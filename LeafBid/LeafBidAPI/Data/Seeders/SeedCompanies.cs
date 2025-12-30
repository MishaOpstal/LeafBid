using Microsoft.EntityFrameworkCore;
using LeafBidAPI.Models;

namespace LeafBidAPI.Data.seeders;

public class SeedCompanies
{
    public static async Task SeedCompaniesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        await SeedCompanyAsync(context, "AholdDelhaize", "Zaandam", "NL", "12", "A", "2582 DJ", "Hoogstraat", cancellationToken);
        await SeedCompanyAsync(context, "JeLokaleBloemenmarkt", "Leiden", "NL", "12", "", "2251 SN", "Apollolaan", cancellationToken);
        await SeedCompanyAsync(context, "RoffaBloemen", "Rotterdam", "NL", "12", "", "3011 CD", "Kapelstraat", cancellationToken);
    }

    private static async Task SeedCompanyAsync(
        ApplicationDbContext context,
        string name,
        string city,
        string countryCode,
        string houseNumber,
        string houseNumberSuffix,
        string postalCode,
        string street,
        CancellationToken cancellationToken)
    {
        if (await context.Companies.AnyAsync(c => c.Name == name, cancellationToken))
            return;

        context.Companies.Add(new Company
        {
            City = city,
            Name = name,
            CountryCode = countryCode,
            HouseNumber = houseNumber,
            HouseNumberSuffix = houseNumberSuffix,
            PostalCode = postalCode,
            Street = street
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}