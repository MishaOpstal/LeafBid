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
            .Include(ap => ap.RegisteredProduct)
                .ThenInclude(rp => rp!.Product)
            .Include(ap => ap.RegisteredProduct)
                .ThenInclude(rp => rp!.User)
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
            .Include(ap => ap.RegisteredProduct)
                .ThenInclude(rp => rp!.Product)
            .Include(ap => ap.RegisteredProduct)
                .ThenInclude(rp => rp!.User)
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
        
       
   // TODO: remove hardcoded date once product goes live
    DateTime today = new DateTime(2019, 12, 29);
    DateTime tomorrow = today.AddDays(1);
    

    var auctions = await context.Auctions
        .Where(a => a.StartDate >= today && a.StartDate < tomorrow)
        .OrderBy(a => a.ClockLocationEnum)
        .ToListAsync();
    

    if (!auctions.Any())
    {
        throw new NotFoundException("No auctions found for today.");
    }

    var productService = new ProductService(context, userManager);
    var auctionDtos = new List<GetAuctionWithProductsDto>();

    foreach (var auction in auctions)
    {
        var registeredProducts = await context.AuctionProducts
            .Where(ap => ap.AuctionId == auction.Id && ap.RegisteredProductId != null)
                .Include(ap => ap.RegisteredProduct)
                    .ThenInclude(rp => rp!.Product) // Add this
                .Include(ap => ap.RegisteredProduct)
                    .ThenInclude(rp => rp!.User)    // Add this
            .OrderBy(ap => ap.ServeOrder)
            .Select(ap => ap.RegisteredProduct!)
            .ToListAsync();

        if (!registeredProducts.Any())
        {
            continue; // skip this auction but keep processing others
        }

        var registeredProductResponses = new List<RegisteredProductResponse>();
        foreach (var rp in registeredProducts)
        {
            try
            {
                registeredProductResponses.Add(productService.CreateRegisteredProductResponse(rp));
            }
            catch (Exception ex)
            {
                // continue processing remaining products, this is normal flow
            }
        }

        auctionDtos.Add(new GetAuctionWithProductsDto
        {
            Auction = auction,
            RegisteredProducts = registeredProductResponses
        });
        
    }

    var ordered = auctionDtos
        .OrderBy(dto => dto.Auction.ClockLocationEnum)
        .ToList();
    
    return ordered;    
    }
}
