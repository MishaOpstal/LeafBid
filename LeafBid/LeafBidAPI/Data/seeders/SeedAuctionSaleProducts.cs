using LeafBidAPI.Enums;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedAuctionSaleProducts
{
    public static async Task SeedAuctionSaleProductsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var auctioneerId = await context.Users
            .Where(u => u.UserName == "Auctioneer")
            .Select(u => u.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        await SeedAuctionSaleProductAsync(context, auctioneerId, ClockLocationEnum.Naaldwijk, "TestProduct1", 3, 2.50m, cancellationToken);
        await SeedAuctionSaleProductAsync(context, auctioneerId, ClockLocationEnum.Naaldwijk, "TestProduct3", 3, 2.50m, cancellationToken);
        await SeedAuctionSaleProductAsync(context, auctioneerId, ClockLocationEnum.Aalsmeer, "TestProduct2", 5, 2.50m, cancellationToken);
    }

    private static async Task SeedAuctionSaleProductAsync(
        ApplicationDbContext context,
        string auctioneerId,
        ClockLocationEnum clockLocation,
        string productName,
        int quantity,
        decimal price,
        CancellationToken cancellationToken)
    {
        var auctionSaleId = await context.AuctionSales
            .Where(aus => aus.Auction.UserId == auctioneerId && aus.Auction.ClockLocationEnum == clockLocation)
            .Select(aus => aus.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        var productId = await context.Products
            .Where(p => p.Name == productName)
            .Select(p => p.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        if (await context.AuctionSaleProducts.AnyAsync(asp => asp.AuctionSaleId == auctionSaleId && asp.ProductId == productId, cancellationToken))
            return;

        context.AuctionSaleProducts.Add(new AuctionSaleProduct
        {
            AuctionSaleId = auctionSaleId,
            ProductId = productId,
            Quantity = quantity,
            Price = price
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}