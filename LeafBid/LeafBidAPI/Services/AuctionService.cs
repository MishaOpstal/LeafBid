using System.Security.Claims;
using LeafBidAPI.Data;
using LeafBidAPI.DTOs.Auction;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Services;

public class AuctionService(
    ApplicationDbContext context,
    UserManager<User> userManager) : IAuctionService
{
    public async Task<List<Auction>> GetAuctions()
    {
        return await context.Auctions.ToListAsync();
    }
    
    public async Task<Auction> GetAuctionById(int id)
    {
        Auction? auction = await context.Auctions.FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null)
        {
            throw new NotFoundException("Auction not found");
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

        IList<string> roles = await userManager.GetRolesAsync(currentUser);
        if (!roles.Contains("Auctioneer"))
        {
            throw new UnauthorizedAccessException("User does not have the required role to create an auction");
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
