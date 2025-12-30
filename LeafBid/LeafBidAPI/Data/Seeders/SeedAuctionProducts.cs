using LeafBidAPI.Enums;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedAuctionProducts
{
    public static async Task SeedAuctionProductsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        string auctioneerId1 = await context.Users
            .Where(u => u.UserName == "Auctioneer1")
            .Select(u => u.Id)
            .FirstAsync(cancellationToken: cancellationToken);
        
        string auctioneerId2 = await context.Users
            .Where(u => u.UserName == "Auctioneer2")
            .Select(u => u.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        await SeedAuctionProductAsync(context, auctioneerId1, ClockLocationEnum.Naaldwijk, "TestProduct1", 10, 20, cancellationToken);
        await SeedAuctionProductAsync(context, auctioneerId1, ClockLocationEnum.Naaldwijk, "TestProduct2", 9, 100, cancellationToken);
        await SeedAuctionProductAsync(context, auctioneerId2, ClockLocationEnum.Aalsmeer, "TestProduct3", 10, 25, cancellationToken);
        await SeedAuctionProductAsync(context, auctioneerId2, ClockLocationEnum.Aalsmeer, "TestProduct4", 9, 10, cancellationToken);
    }

    private static async Task SeedAuctionProductAsync(
        ApplicationDbContext context,
        string auctioneerId,
        ClockLocationEnum clockLocation,
        string productName,
        int serveOrder,
        int auctionStock,
        CancellationToken cancellationToken)
    {
        var auctionId = await context.Auctions
            .Where(a => a.UserId == auctioneerId && a.ClockLocationEnum == clockLocation)
            .Select(a => a.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        var registeredProductId = await context.RegisteredProducts
            .Where(rp => rp.Product.Name == productName)
            .Select(rp => rp.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        if (await context.AuctionProducts.AnyAsync(ap => ap.AuctionId == auctionId && ap.RegisteredProductId == registeredProductId, cancellationToken))
            return;

        context.AuctionProducts.Add(new AuctionProduct
        {
            AuctionId = auctionId,
            RegisteredProductId = registeredProductId,
            ServeOrder = serveOrder,
            AuctionStock = auctionStock
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}