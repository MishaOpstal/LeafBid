using Microsoft.EntityFrameworkCore;
using LeafBidAPI.Models;

namespace LeafBidAPI.Data.seeders;

public class SeedCompanies
{
    public static async Task SeedCompaniesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(cancellationToken))
            return;

        context.Companies.AddRange(
            new Company
            {
                City = "Zaandam",
                Name = "AholdDelhaize",
                CountryCode = "NL",
                HouseNumber = "12",
                HouseNumberSuffix = "A",
                PostalCode = "2582 DJ",
                Street = "Hoogstraat"
            },
            
            new Company
            {
                City = "Leiden",
                Name = "JeLokaleBloemenmarkt",
                CountryCode = "NL",
                HouseNumber = "12",
                HouseNumberSuffix = "",
                PostalCode = "2251 SN",
                Street = "Apollolaan"
            },
            
            new Company
            {
                City = "Rotterdam",
                Name = "RoffaBloemen",
                CountryCode = "NL",
                HouseNumber = "12",
                HouseNumberSuffix = "",
                PostalCode = "3011 CD",
                Street = "Kapelstraat"
            }
        );
    }
}