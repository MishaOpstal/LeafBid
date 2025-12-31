using Bogus;
using LeafBidAPI.Enums;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace LeafBidAPI.Data.factories;

public class AuctionSaleFactory(
    ApplicationDbContext dbContext,
    UserManager<User> userManager
) : Factory<AuctionSale>
{
    private readonly IList<Auction> _auctions =
        dbContext.Auctions.Where(a => a.StartDate < DateTime.UtcNow).ToList();
    private readonly IList<User> _users = userManager.GetUsersInRoleAsync("Buyer").Result;
    
    protected override Faker<AuctionSale> BuildRules()
    {
        Faker faker = new();
        // If no buyers are found, abort.
        if (_users.Count == 0)
        {
            throw new NotFoundException("No users with buyer role found, aborting auction sale factory generation");
        }
        
        // If no auctions are found, abort.
        if (_auctions.Count == 0)
        {
            throw new NotFoundException("No auctions found, aborting auction sale factory generation");
        }

        DateTime auctionDate = faker.Random.ListItem(_auctions).StartDate;
        DateTime saleDate = faker.Date.Between(auctionDate, DateTime.UtcNow);
        
        return new Faker<AuctionSale>()
            .RuleFor(
                asa => asa.UserId,
                f => f.Random.ListItem(_users).Id
            )
            .RuleFor(
                asa => asa.AuctionId,
                f => f.Random.ListItem(_auctions).Id
            )
            .RuleFor(
                asa => asa.Date,
                saleDate
            )
            .RuleFor(
                asa => asa.PaymentReference,
                f => f.Random.String(10)
            );
    }
}