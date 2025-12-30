using LeafBidAPI.Enums;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedAuctionProducts
{
    public static async Task SeedAuctionProductsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(cancellationToken))
            return;
        
        string auctioneerId = await context.Users
            .Where(u => u.UserName == "Auctioneer")
            .Select(u => u.Id)
            .FirstAsync(cancellationToken: cancellationToken);
        
        List<Product> testProducts = await context.Products
            .Where(p => p.Name.Contains("TestProduct"))
            .ToListAsync(cancellationToken: cancellationToken);

        context.AuctionProducts.AddRange(
            new AuctionProduct
            {
                AuctionId = await context.Auctions
                    .Where(a => a.UserId == auctioneerId && a.ClockLocationEnum == ClockLocationEnum.Naaldwijk)
                    .Select(a => a.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                RegisteredProductId = testProducts[0].Id,
                ServeOrder = 10,
                AuctionStock = 20
            },
            
            new AuctionProduct
            {
                AuctionId = await context.Auctions
                    .Where(a => a.UserId == auctioneerId && a.ClockLocationEnum == ClockLocationEnum.Naaldwijk)
                    .Select(a => a.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                RegisteredProductId = testProducts[1].Id,
                ServeOrder = 9,
                AuctionStock = 100
            },
            
            new AuctionProduct
            {
                AuctionId = await context.Auctions
                    .Where(a => a.UserId == auctioneerId && a.ClockLocationEnum == ClockLocationEnum.Aalsmeer)
                    .Select(a => a.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                RegisteredProductId = testProducts[3].Id,
                ServeOrder = 10,
                AuctionStock = 25
            },
            
            new AuctionProduct
            {
                AuctionId = await context.Auctions
                    .Where(a => a.UserId == auctioneerId && a.ClockLocationEnum == ClockLocationEnum.Aalsmeer)
                    .Select(a => a.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                RegisteredProductId = testProducts[4].Id,
                ServeOrder = 9,
                AuctionStock = 10
            }

        );
    }
}