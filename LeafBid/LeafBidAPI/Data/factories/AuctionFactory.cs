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
        Faker faker = new();
        DateTime auctionDate = faker.Date.Between(faker.Date.Past(8, DateTime.UtcNow), faker.Date.Future());

        if (_auctionSales.Count == 0)
        {
            auctionDate = faker.Date.Past(8, DateTime.UtcNow);
        }
        
        bool futureDate = auctionDate > DateTime.UtcNow;
        
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
                auctionDate
            )
            .RuleFor(
                a => a.IsLive,
                futureDate
            )
            .RuleFor(
                a => a.ClockLocationEnum,
                f => f.Random.Enum<ClockLocationEnum>()
            );
    }
}