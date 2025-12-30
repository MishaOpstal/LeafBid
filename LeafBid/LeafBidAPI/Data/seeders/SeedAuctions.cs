using LeafBidAPI.Enums;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedAuctions
{
    public static async Task SeedAuctionsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var auctioneerId = await context.Users
            .Where(u => u.UserName == "Auctioneer")
            .Select(u => u.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        await SeedAuctionAsync(context, auctioneerId, ClockLocationEnum.Aalsmeer, DateTime.UtcNow.AddDays(1), cancellationToken);
        await SeedAuctionAsync(context, auctioneerId, ClockLocationEnum.Naaldwijk, DateTime.UtcNow.AddDays(2), cancellationToken);
    }

    private static async Task SeedAuctionAsync(
        ApplicationDbContext context,
        string userId,
        ClockLocationEnum clockLocation,
        DateTime startDate,
        CancellationToken cancellationToken)
    {
        if (await context.Auctions.AnyAsync(a => a.UserId == userId && a.ClockLocationEnum == clockLocation && a.StartDate.Date == startDate.Date, cancellationToken))
            return;

        context.Auctions.Add(new Auction
        {
            ClockLocationEnum = clockLocation,
            StartDate = startDate,
            IsLive = true,
            UserId = userId
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}