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

    public async Task<GetAuctionPerActiveClockLocationDto> GetAuctionPerActiveClockLocation()
    {
        // return list of auctions with products (running through for each loop using our clock location enums)
        List<Auction> auctions = await context.Auctions
            .OrderBy(a => a.ClockLocationEnum)
            .ToListAsync();

        if (auctions.Count == 0) throw new NotFoundException("No live auctions found.");

        ProductService productService = new ProductService(context, userManager);
        List<GetAuctionWithProductsDto> auctionDtos = new List<GetAuctionWithProductsDto>();

        foreach (Auction auction in auctions)
        {
            List<RegisteredProduct?> RegisteredProducts = await context.AuctionProducts
                .Where(ap => ap.AuctionId == auction.Id)
                .OrderBy(ap => ap.ServeOrder)
                .Select(ap => ap.RegisteredProduct)
                .Where(p => p != null)
                .ToListAsync();

            List<RegisteredProductResponse> registeredProductResponses = RegisteredProducts
                .OfType<RegisteredProduct>()
                .Select(rp => productService.CreateRegisteredProductResponse(rp))
                .ToList();
            
            auctionDtos.Add(new GetAuctionWithProductsDto
            {
                Auction = auction,
                RegisteredProducts = registeredProductResponses
            });
        }

        return new GetAuctionPerActiveClockLocationDto
        {
            Auctions = auctionDtos
        };
    }
}
