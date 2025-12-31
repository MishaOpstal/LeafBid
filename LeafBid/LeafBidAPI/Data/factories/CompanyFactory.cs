using Bogus;
using LeafBidAPI.Models;

namespace LeafBidAPI.Data.factories;

public class CompanyFactory : Factory<Company>
{
    protected override Faker<Company> BuildRules()
    {
        return new Faker<Company>()
            .RuleFor(
                c => c.Name,
                f => f.Company.CompanyName()
            )
            .RuleFor(
                c => c.Street,
                f => f.Address.StreetAddress()
            )
            .RuleFor(
                c => c.City,
                f => f.Address.City()
            )
            .RuleFor(
                c => c.HouseNumber,
                f => f.Address.BuildingNumber()
            )
            .RuleFor(
                c => c.HouseNumberSuffix,
                f => f.Address.StreetSuffix()
            )
            .RuleFor(
                c => c.PostalCode,
                f => f.Address.ZipCode()
            )
            .RuleFor(
                c => c.CountryCode,
                f => f.Address.CountryCode()
            );
    }
}