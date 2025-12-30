using LeafBidAPI.Enums;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedAuctionSales
{
    public static async Task SeedAuctionSalesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(cancellationToken))
            return;

        string auctioneerId = await context.Users
            .Where(u => u.UserName == "Auctioneer")
            .Select(u => u.Id)
            .FirstAsync(cancellationToken: cancellationToken);
        
        context.AuctionSales.AddRange(
            new AuctionSale
            {
                AuctionId = await context.Auctions
                    .Where(a => a.ClockLocationEnum == ClockLocationEnum.Aalsmeer && a.UserId == auctioneerId)
                    .Select(a => a.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                UserId = await context.Users
                    .Where(u => u.UserName == "Buyer")
                    .Select(u => u.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                PaymentReference = "1234567890"
            },
            new AuctionSale
            {
                AuctionId = await context.Auctions
                    .Where(a => a.ClockLocationEnum == ClockLocationEnum.Naaldwijk && a.UserId == auctioneerId)
                    .Select(a => a.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                UserId = await context.Users
                    .Where(u => u.UserName == "Buyer")
                    .Select(u => u.Id)
                    .FirstAsync(cancellationToken: cancellationToken),
                PaymentReference = "0987654321"
            }
            );
    }
}