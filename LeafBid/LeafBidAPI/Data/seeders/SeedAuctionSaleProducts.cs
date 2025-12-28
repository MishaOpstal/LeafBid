using LeafBidAPI.Enums;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedAuctionSaleProducts
{
    public static async Task SeedAuctionSaleProductsAsync(ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(cancellationToken))
            return;
        
        var AuctioneerID = await context.Users
            .Where(u => u.UserName == "Auctioneer")
            .Select(u => u.Id)
            .FirstAsync(cancellationToken: cancellationToken);
        
        context.AuctionSaleProducts.AddRange(
            new AuctionSaleProduct
            {
                AuctionSaleId = await context.AuctionSales
                    .Where(aus => aus.Auction.UserId == AuctioneerID && aus.Auction.ClockLocationEnum == ClockLocationEnum.Naaldwijk)
                    .Select(aus => aus.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                ProductId = await context.Products
                    .Where(p => p.Name == "TestProduct1")
                    .Select(p => p.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                Quantity = 3,
                Price = 2.50m
            },
            new AuctionSaleProduct
            {
                AuctionSaleId = await context.AuctionSales
                    .Where(aus => aus.Auction.UserId == AuctioneerID && aus.Auction.ClockLocationEnum == ClockLocationEnum.Naaldwijk)
                    .Select(aus => aus.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                ProductId = await context.Products
                    .Where(p => p.Name == "TestProduct3")
                    .Select(p => p.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                Quantity = 3,
                Price = 2.50m
            },
            new AuctionSaleProduct
            {
                
                AuctionSaleId = await context.AuctionSales
                    .Where(aus => aus.Auction.UserId == AuctioneerID && aus.Auction.ClockLocationEnum == ClockLocationEnum.Aalsmeer)
                    .Select(aus => aus.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                ProductId = await context.Products
                    .Where(p => p.Name == "TestProduct2")
                    .Select(p => p.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                Quantity = 5,
                Price = 2.50m
            }
            );
    }
}