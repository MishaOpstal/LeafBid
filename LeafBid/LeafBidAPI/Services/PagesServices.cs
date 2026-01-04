using LeafBidAPI.Data;
using LeafBidAPI.DTOs.Page;
using LeafBidAPI.DTOs.RegisteredProduct;
using LeafBidAPI.Enums;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Services;

public class PagesServices(
    ApplicationDbContext context,
    UserManager<User> userManager,
    ILogger<PagesServices> logger
) : IPagesServices
{
    public async Task<GetAuctionWithProductsDto> GetAuctionWithProducts(ClockLocationEnum clockLocation)
    {
        Auction? auction = await context.Auctions
            .Where(a => a.ClockLocationEnum == clockLocation)
            .OrderBy(a => a.StartDate)
            .FirstOrDefaultAsync();

        if (auction == null)
        {
            throw new NotFoundException("Auction not found.");
        }

        List<RegisteredProduct?> registeredProducts = await context.AuctionProducts
            .Where(ap => ap.AuctionId == auction.Id)
            .OrderBy(ap => ap.ServeOrder)
            .Select(ap => ap.RegisteredProduct)
            .Where(p => p != null)
            .ToListAsync();

        if (registeredProducts.Count == 0)
        {
            throw new NotFoundException("No registered products found for this auction.");
        }

        ProductService productService = new(context, userManager);
        List<RegisteredProductResponse> registeredProductResponses = registeredProducts
            .OfType<RegisteredProduct>()
            .Select(registeredProduct => productService.CreateRegisteredProductResponse(registeredProduct))
            .ToList();

        GetAuctionWithProductsDto result = new()
        {
            Auction = auction,
            RegisteredProducts = registeredProductResponses
        };

        return result;
    }
    
    public async Task<GetAuctionWithProductsDto> GetAuctionWithProductsById(int auctionId)
    {
        Auction? auction = await context.Auctions
            .Where(a => a.Id == auctionId)
            .FirstOrDefaultAsync();

        if (auction == null)
        {
            throw new NotFoundException("Auction not found.");
        }

        List<RegisteredProduct?> registeredProducts = await context.AuctionProducts
            .Where(ap => ap.AuctionId == auction.Id)
            .OrderBy(ap => ap.ServeOrder)
            .Select(ap => ap.RegisteredProduct)
            .Where(p => p != null)
            .ToListAsync();

        if (registeredProducts.Count == 0)
        {
            throw new NotFoundException("No registered products found for this auction.");
        }

        ProductService productService = new(context, userManager);
        List<RegisteredProductResponse> registeredProductResponses = registeredProducts
            .OfType<RegisteredProduct>()
            .Select(registeredProduct => productService.CreateRegisteredProductResponse(registeredProduct))
            .ToList();

        GetAuctionWithProductsDto result = new()
        {
            Auction = auction,
            RegisteredProducts = registeredProductResponses
        };

        return result;
    }

    public async Task<List<GetAuctionWithProductsDto>> GetAuctionPerActiveClockLocation()
    {
        
        //TODO: fix this entire mess up, make it so that running the endpoint doesn't just keep giving me a NotFoundException
       
        DateTime today = new DateTime(2019, 12, 29); // TODO: remove hardcoded date
        DateTime tomorrow = today.AddDays(1);

        logger.LogInformation("GetAuctionPerActiveClockLocation start - range {Today} to {Tomorrow}", today, tomorrow);

        List<Auction> auctions = await context.Auctions
            .Where(a => a.StartDate >= today && a.StartDate < tomorrow)
            .OrderBy(a => a.ClockLocationEnum)
            .ToListAsync();

        logger.LogInformation("Found {Count} auctions for today range", auctions.Count);

        if (auctions.Count == 0)
        {
            logger.LogWarning("No auctions found for today range {Today}-{Tomorrow}", today, tomorrow);
            throw new NotFoundException("No auctions found for today.");
        }

        ProductService productService = new ProductService(context, userManager);
        List<GetAuctionWithProductsDto> auctionDtos = new List<GetAuctionWithProductsDto>();

        foreach (Auction auction in auctions)
        {
            List<RegisteredProduct?> registeredProducts = await context.AuctionProducts
                .Where(ap => ap.AuctionId == auction.Id)
                .OrderBy(ap => ap.ServeOrder)
                .Select(ap => ap.RegisteredProduct)
                .Where(p => p != null)
                .ToListAsync();

            if (registeredProducts.Count == 0)
            {
                logger.LogWarning("No registered products for auction {AuctionId} starting {StartDate}", auction.Id, auction.StartDate);
                throw new NotFoundException($"No registered products found for auction {auction.Id}.");
            }

            List<RegisteredProductResponse> registeredProductResponses = registeredProducts
                .OfType<RegisteredProduct>()
                .Select(rp => productService.CreateRegisteredProductResponse(rp))
                .ToList();

            auctionDtos.Add(new GetAuctionWithProductsDto
            {
                Auction = auction,
                RegisteredProducts = registeredProductResponses
            });

            logger.LogInformation("Processed auction {AuctionId} with {Count} products", auction.Id, registeredProductResponses.Count);
        }

        var ordered = auctionDtos
            .OrderBy(dto => dto.Auction.ClockLocationEnum)
            .ToList();

        logger.LogInformation("GetAuctionPerActiveClockLocation completed - returning {Count} DTOs", ordered.Count);
        return ordered;
    }
}
