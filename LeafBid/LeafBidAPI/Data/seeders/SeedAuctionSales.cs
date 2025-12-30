using LeafBidAPI.Enums;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedAuctionSales
{
    public static async Task SeedAuctionSalesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        string auctioneerId = await context.Users
            .Where(u => u.UserName == "Auctioneer")
            .Select(u => u.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        string buyerId = await context.Users
            .Where(u => u.UserName == "Buyer1")
            .Select(u => u.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        await SeedAuctionSaleAsync(context, auctioneerId, buyerId, ClockLocationEnum.Aalsmeer, "1234567890", cancellationToken);
        await SeedAuctionSaleAsync(context, auctioneerId, buyerId, ClockLocationEnum.Naaldwijk, "0987654321", cancellationToken);
    }

    private static async Task SeedAuctionSaleAsync(
        ApplicationDbContext context,
        string auctioneerId,
        string buyerId,
        ClockLocationEnum clockLocation,
        string paymentReference,
        CancellationToken cancellationToken)
    {
        var auctionId = await context.Auctions
            .Where(a => a.ClockLocationEnum == clockLocation && a.UserId == auctioneerId)
            .Select(a => a.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        if (await context.AuctionSales.AnyAsync(asale => asale.AuctionId == auctionId && asale.UserId == buyerId && asale.PaymentReference == paymentReference, cancellationToken))
            return;

        context.AuctionSales.Add(new AuctionSale
        {
            AuctionId = auctionId,
            UserId = buyerId,
            PaymentReference = paymentReference
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}