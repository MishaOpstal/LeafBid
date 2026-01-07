using LeafBidAPI.Data;
using LeafBidAPI.DTOs.Page;
using LeafBidAPI.DTOs.RegisteredProduct;
using LeafBidAPI.Enums;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LeafBidAPI.Helpers;

namespace LeafBidAPI.Services;

public class PagesServices(
    ApplicationDbContext context,
    UserManager<User> userManager
) : IPagesServices
{
    public async Task<List<GetAuctionWithProductsResponse>> GetClosestAuctionsWithProducts(ClockLocationEnum clockLocation)
    {
        List<Auction> auctions = await context.Auctions
            .Where(a => a.ClockLocationEnum == clockLocation)
            .OrderBy(a => a.StartDate)
            .ToListAsync();

        if (auctions.Count == 0)
        {
            throw new NotFoundException("Auctions not found for today at the specified clock location.");
        }

        ProductService productService = new(context, userManager);
        List<GetAuctionWithProductsResponse> responses = [];

        foreach (Auction auction in auctions)
        {
            List<RegisteredProduct> registeredProducts = await GetRegisteredProductsForAuction(auction.Id);
            if (registeredProducts.Count == 0)
            {
                continue;
            }

            List<RegisteredProductResponse> productResponses =
                CreateRegisteredProductResponses(productService, registeredProducts);

            responses.Add(new GetAuctionWithProductsResponse
            {
                Auction = auction,
                RegisteredProducts = productResponses,
                ServerTime = TimeHelper.GetAmsterdamTime()
            });
        }

        return responses
            .OrderBy(dto => dto.Auction.ClockLocationEnum)
            .ToList();
    }

    public async Task<GetAuctionWithProductsResponse> GetAuctionWithProductsById(int auctionId)
    {
        Auction? auction = await context.Auctions
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction == null)
        {
            throw new NotFoundException("Auction not found.");
        }

        List<RegisteredProduct> registeredProducts = await GetRegisteredProductsForAuction(auction.Id);

        if (registeredProducts.Count == 0)
        {
            throw new NotFoundException("No registered products found for this auction.");
        }

        ProductService productService = new(context, userManager);

        List<RegisteredProductResponse> productResponses =
            CreateRegisteredProductResponses(productService, registeredProducts);

        return new GetAuctionWithProductsResponse
        {
            Auction = auction,
            RegisteredProducts = productResponses,
            ServerTime = TimeHelper.GetAmsterdamTime()
        };
    }

    public async Task<List<GetAuctionWithProductsResponse>> GetAuctionsWithProductsPerClockLocation()
    {
        List<Auction> auctions = await context.Auctions
            .OrderBy(a => a.ClockLocationEnum)
            .ToListAsync();

        if (auctions.Count == 0)
        {
            throw new NotFoundException("No auctions found for today.");
        }

        ProductService productService = new(context, userManager);
        List<GetAuctionWithProductsResponse> responses = [];

        foreach (Auction auction in auctions)
        {
            List<RegisteredProduct> registeredProducts = await GetRegisteredProductsForAuction(auction.Id);
            if (registeredProducts.Count == 0)
            {
                continue;
            }

            List<RegisteredProductResponse> productResponses =
                CreateRegisteredProductResponses(productService, registeredProducts);

            responses.Add(new GetAuctionWithProductsResponse
            {
                Auction = auction,
                RegisteredProducts = productResponses,
                ServerTime = TimeHelper.GetAmsterdamTime()
            });
        }

        return responses
            .OrderBy(dto => dto.Auction.ClockLocationEnum)
            .ToList();
    }

    private async Task<List<RegisteredProduct>> GetRegisteredProductsForAuction(int auctionId)
    {
        return await context.AuctionProducts
            .Where(ap => ap.AuctionId == auctionId && ap.RegisteredProduct!.Stock > 0)
            .Include(ap => ap.RegisteredProduct)
            .ThenInclude(rp => rp!.Product)
            .Include(ap => ap.RegisteredProduct)
            .ThenInclude(rp => rp!.User)
            .OrderBy(ap => ap.ServeOrder)
            .Select(ap => ap.RegisteredProduct!)
            .ToListAsync();
    }

    private static List<RegisteredProductResponse> CreateRegisteredProductResponses(
        ProductService productService,
        List<RegisteredProduct> registeredProducts)
    {
        List<RegisteredProductResponse> responses = [];

        foreach (RegisteredProduct registeredProduct in registeredProducts)
        {
            try
            {
                responses.Add(productService.CreateRegisteredProductResponse(registeredProduct));
            }
            catch (Exception ex)
            {
                throw new NotFoundException(
                    $"Failed to create RegisteredProductResponse for RegisteredProduct {registeredProduct.Id}", ex);
            }
        }

        return responses;
    }
}
