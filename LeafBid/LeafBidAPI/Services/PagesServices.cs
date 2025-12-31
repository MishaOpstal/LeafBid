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
    UserManager<User> userManager
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
}
