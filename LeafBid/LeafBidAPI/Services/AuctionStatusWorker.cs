using LeafBidAPI.Data;
using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.Helpers;
using LeafBidAPI.Hubs;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Services;

public class AuctionStatusWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<AuctionStatusWorker> logger,
    IHubContext<AuctionHub> hubContext
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Auction Status Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateAuctionStatuses();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating auction statuses.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        logger.LogInformation("Auction Status Worker is stopping.");
    }

    private async Task UpdateAuctionStatuses()
    {
        using IServiceScope scope = scopeFactory.CreateScope();

        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IProductService productService =
            scope.ServiceProvider.GetRequiredService<IProductService>();

        AuctionHelper auctionHelper =
            scope.ServiceProvider.GetRequiredService<AuctionHelper>();

        DateTime now = TimeHelper.GetAmsterdamTime();
        DateTime visibilityThreshold = now.AddHours(2);

        // 0. Set IsVisible = true for auctions that should be visible (starting within 2 hours) and have stock
        List<Auction> auctionsToMakeVisible = await context.Auctions
            .Where(a => !a.IsVisible && a.StartDate <= visibilityThreshold)
            .Where(a => context.AuctionProducts.Any(ap => ap.AuctionId == a.Id && ap.RegisteredProduct!.Stock > 0))
            .ToListAsync();

        foreach (Auction auction in auctionsToMakeVisible.Where(auction => auction.DeletedAt == null))
        {
            auction.IsVisible = true;
            logger.LogInformation("Auction {AuctionId} is now Visible.", auction.Id);
        }

        // 1. Set IsLive = true for auctions that should start and have products with stock
        List<Auction> auctionsToStart = await context.Auctions
            .Where(a => !a.IsLive && a.StartDate <= now)
            .Where(a => context.AuctionProducts.Any(ap => ap.AuctionId == a.Id && ap.RegisteredProduct!.Stock > 0))
            .ToListAsync();

        foreach (Auction auction in auctionsToStart.Where(auction => auction.DeletedAt == null))
        {
            // Check if an auction at the same clock location is already live,
            // if so, push the startdate of this auction forwards to 30 minutes
            Auction? existingLiveAuction = await context.Auctions
                .Where(a => a.IsLive && a.ClockLocationEnum == auction.ClockLocationEnum)
                .FirstOrDefaultAsync();

            if (existingLiveAuction == null)
            {
                auction.IsLive = true;
                auction.IsVisible = true; // Ensure it's visible if it's live
                auction.NextProductStartTime = auction.StartDate; // Initialize the timer
                logger.LogInformation("Auction {AuctionId} is now Live.", auction.Id);

                // Send out an AuctionStarted event
                await hubContext.Clients.Group(auction.Id.ToString()).SendAsync("AuctionStarted", new
                {
                    auctionId = auction.Id
                });
                
                continue;
            }

            auction.StartDate = TimeHelper.GetAmsterdamTime().AddMinutes(30);
            context.Auctions.Update(auction);
        }
        
        // Make sure all deleted auctions are not live or visible
        List<Auction> deletedAuctions = await context.Auctions.Where(a => a.DeletedAt != null).ToListAsync();
        foreach (Auction auction in deletedAuctions)
        {
            auction.IsLive = false;
            auction.IsVisible = false;
            auction.NextProductStartTime = null;
            
            // Remove all associated AuctionProducts from the deleted auction
            await context.AuctionProducts.Where(ap => ap.AuctionId == auction.Id).ExecuteDeleteAsync();
            
            await context.SaveChangesAsync();
        }

        // 1.5 Process Expirations for live auctions
        List<Auction> liveAuctions = await context.Auctions
            .Where(a => a.IsLive)
            .ToListAsync();

        foreach (Auction auction in liveAuctions)
        {
            if (auction.NextProductStartTime == null || now < auction.NextProductStartTime)
            {
                continue;
            }

            RegisteredProduct? activeProduct = await context.AuctionProducts
                .Where(ap => ap.AuctionId == auction.Id && ap.RegisteredProduct!.Stock > 0)
                .OrderBy(ap => ap.ServeOrder)
                .Include(ap => ap.RegisteredProduct)
                .Select(ap => ap.RegisteredProduct)
                .FirstOrDefaultAsync();

            if (activeProduct == null)
            {
                continue;
            }

            double duration = auctionHelper.GetProductDurationSeconds(activeProduct);
            double elapsed = (now - auction.NextProductStartTime.Value).TotalSeconds;

            if (!(elapsed >= duration))
            {
                continue;
            }

            logger.LogInformation(
                "Auction {AuctionId} product {ProductId} expired automatically (duration: {Duration}s, elapsed: {Elapsed}s).",
                auction.Id, activeProduct.Id, duration, Math.Round(elapsed, 2));

            AuctionEventResponse result = await productService.ExpireProduct(activeProduct.Id, auction.Id);

            if (result.IsSuccess)
            {
                await hubContext.Clients.Group(auction.Id.ToString()).SendAsync("ProductExpired", new
                {
                    registeredProductId = result.RegisteredProduct.Id,
                    nextProductStartTime = result.NextProductStartTime
                });
            }
        }

        // 2. Set IsLive and IsVisible = false for live auctions with no products with stock remaining
        List<Auction> finishedAuctions = await context.Auctions
            .Where(a => a.IsLive)
            .Where(a => !context.AuctionProducts.Any(ap => ap.AuctionId == a.Id && ap.RegisteredProduct!.Stock > 0))
            .ToListAsync();

        foreach (Auction auction in finishedAuctions)
        {
            auction.IsLive = false;
            auction.IsVisible = false;
            logger.LogInformation("Auction {AuctionId} is no longer Live (no stock remaining).", auction.Id);

            // Send out an AuctionStopped event
            await hubContext.Clients.Group(auction.Id.ToString()).SendAsync("AuctionStopped", new
            {
                auctionId = auction.Id,
                reason = "No products with stock remaining"
            });
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}