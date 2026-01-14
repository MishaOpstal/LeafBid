using System.Security.Claims;
using LeafBidAPI.Data;
using LeafBidAPI.DTOs.Auction;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Helpers;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Services;

public class AuctionService(
    ApplicationDbContext context,
    UserManager<User> userManager) : IAuctionService
{
    public async Task<List<Auction>> GetAuctions(ClaimsPrincipal user)
    {
        bool canSeeDeleted =
            user.IsInRole("Auctioneer") ||
            user.IsInRole("Admin");
        
        IQueryable<Auction> query = context.Auctions;
        
        if (!canSeeDeleted)
        {
            query = query.Where(a => a.DeletedAt == null);
        }

        return await query.ToListAsync();
    }
    
    public async Task<Auction> GetAuctionById(int id, ClaimsPrincipal user)
    {
        Auction? auction = await context.Auctions.FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null)
        {
            throw new NotFoundException("Auction not found");
        }
        
        // Check whether the user is an auctioneer or admin
        if (auction.DeletedAt != null && (!user.IsInRole("Auctioneer") || !user.IsInRole("Admin")))
        {
            throw new UnauthorizedAccessException("User is not authorized to access this auction");
        }

        return auction;
    }
    
    public async Task<Auction> CreateAuction(CreateAuctionDto auctionData, ClaimsPrincipal user)
    {
        User? currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            throw new NotFoundException("User not found");
        }

        foreach (RegisteredProductForAuctionRequest registeredProductForAuction in auctionData.RegisteredProductsForAuction)
        {
            AuctionProduct? auctionProducts =
                await context.AuctionProducts.FirstOrDefaultAsync(a => a.RegisteredProductId == registeredProductForAuction.Id);
            if (auctionProducts != null)
            {
                throw new ProductAlreadyAssignedException("Registered product already assigned");
            }
        }

        Auction auction = new()
        {
            UserId = currentUser.Id,
            ClockLocationEnum = auctionData.ClockLocationEnum,
            StartDate = auctionData.StartDate
        };

        context.Auctions.Add(auction);
        await context.SaveChangesAsync();

        int counter = 1;
        foreach (RegisteredProductForAuctionRequest registeredProductForAuction in auctionData.RegisteredProductsForAuction)
        {
            AuctionProduct auctionProduct = new()
            {
                AuctionId = auction.Id,
                RegisteredProductId = registeredProductForAuction.Id,
                ServeOrder = counter++,
            };

            context.AuctionProducts.Add(auctionProduct);
            
            RegisteredProduct? registeredProduct = await context.RegisteredProducts.FirstOrDefaultAsync(rp => rp.Id == registeredProductForAuction.Id);
            if (registeredProduct == null)
            {
                throw new NotFoundException("Registered product not found");
            }
            
            registeredProduct.MaxPrice = registeredProductForAuction.MaxPrice;
            context.RegisteredProducts.Update(registeredProduct);
        }

        await context.SaveChangesAsync();

        return auction;
    }
    
    public async Task<Auction> StartAuction(int id)
    {
        Auction? auction = await context.Auctions.FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
        {
            throw new NotFoundException("Auction not found");
        }

        if (auction.IsLive)
        {
            throw new AuctionAlreadyStartedAtClockLocationException("Auction already started");
        }

        // Set the StartDate to 10 seconds from now
        auction.StartDate = TimeHelper.GetAmsterdamTime().AddSeconds(10);
        
        await context.SaveChangesAsync();

        return auction;
    }

    public async Task<bool> StopAuction(int id)
    {
        Auction? auction = await context.Auctions.FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null)
        {
            throw new NotFoundException("Auction not found");
        }

        // If the auction is from the past
        if (auction.StartDate < TimeHelper.GetAmsterdamTime() && !auction.IsLive)
        {
            throw new AuctionAlreadyFinishedException("Auction already finished");
        }
        
        auction.DeletedAt = TimeHelper.GetAmsterdamTime();
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Auction> UpdateAuction(int id, UpdateAuctionDto updatedAuction)
    {
        Auction? auction = await context.Auctions.FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
        {
            throw new NotFoundException("Auction not found");
        }

        auction.StartDate = updatedAuction.StartTime;
        auction.ClockLocationEnum = updatedAuction.ClockLocationEnum;
        auction.IsLive = false;
        auction.IsVisible = false;

        await context.SaveChangesAsync();
        return auction;
    }
    
    public async Task<List<RegisteredProduct>> GetRegisteredProductsByAuctionId(int auctionId)
    {
        List<RegisteredProduct?> registeredProducts = await context.AuctionProducts
            .Where(ap => ap.AuctionId == auctionId && ap.RegisteredProduct!.Stock > 0)
            .OrderBy(ap => ap.ServeOrder)
            .Include(ap => ap.RegisteredProduct)
            .ThenInclude(rp => rp!.Product)
            .Select(ap => ap.RegisteredProduct)
            .ToListAsync();

        if (registeredProducts == null || registeredProducts.Count == 0)
        {
            throw new NotFoundException("Product not found");
        }

        return registeredProducts!;
    }
}
