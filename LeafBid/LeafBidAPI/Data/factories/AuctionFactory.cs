using Bogus;
using LeafBidAPI.Enums;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace LeafBidAPI.Data.factories;

public class AuctionFactory(
    ApplicationDbContext dbContext,
    UserManager<User> userManager
) : Factory<Auction>
{
    private readonly IList<AuctionSale> _auctionSales = dbContext.AuctionSales.ToList();
    private readonly IList<User> _users = userManager.GetUsersInRoleAsync("Provider").Result;
    
    protected override Faker<Auction> BuildRules()
    {
        // If no providers are found, abort.
        if (_users.Count == 0)
        {
            throw new NotFoundException("No users with provider role found, aborting auction factory generation");
        }
        
        return new Faker<Auction>()
            .RuleFor(
                a => a.UserId,
                f => f.Random.ListItem(_users).Id
            )
            .RuleFor(
                a => a.StartDate,
                f => _auctionSales.Count == 0
                    ? f.Date.Past(8, DateTime.UtcNow)
                    : f.Date.Between(f.Date.Past(8, DateTime.UtcNow), f.Date.Future())
            )
            .RuleFor(
                a => a.IsLive,
                (f, a) => a.StartDate <= DateTime.UtcNow
            )
            .RuleFor(
                a => a.IsVisible,
                (f, a) => a.StartDate <= DateTime.UtcNow.AddHours(1)
            )
            .RuleFor(
                a => a.ClockLocationEnum,
                f => f.Random.Enum<ClockLocationEnum>()
            );
    }
}