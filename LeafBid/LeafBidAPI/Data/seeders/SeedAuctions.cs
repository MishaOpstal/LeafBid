using LeafBidAPI.Enums;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedAuctions
{
    public static async Task SeedAuctionsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(cancellationToken))
            return;

        context.Auctions.AddRange(
            new Auction
            {
                ClockLocationEnum = ClockLocationEnum.Aalsmeer,
                StartDate = DateTime.UtcNow.AddDays(1),
                IsLive = true,
                UserId = await context.Users
                    .Where(u => u.UserName == "Auctioneer")
                    .Select(u => u.Id)
                    .FirstAsync(cancellationToken: cancellationToken)
            },
            new Auction
            {
                ClockLocationEnum = ClockLocationEnum.Naaldwijk,
                StartDate = DateTime.UtcNow.AddDays(2),
                IsLive = true,
                UserId = await context.Users
                    .Where(u => u.UserName == "Auctioneer")
                    .Select(u => u.Id)
                    .FirstAsync(cancellationToken: cancellationToken)
            }
        );
    }
}