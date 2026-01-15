using Bogus;
using LeafBidAPI.Models;

namespace LeafBidAPI.Data.factories;

public class CompanyFactory : Factory<Company>
{
//pls give us flowery company names :3
    private static readonly string[] Growers =
    {
        "FloraNova",
        "GreenLeaf Growers",
        "Dutch Plant Group",
        "Anthura",
        "Tropica Plants",
        "PlantWorld BV",
        "Kwekerij Groenveld",
        "Westland Flora",
        "Royal Green Farms"
    };
    
    protected override Faker<Company> BuildRules()
    {
        return new Faker<Company>("nl")
            .RuleFor(
                c => c.Name, 
                  f => f.PickRandom(Growers)
            ) 
            .RuleFor(
                c => c.Street, 
                  f => f.Address.StreetAddress()
            ) 
            .RuleFor(
                c => c.City, 
                  f => f.PickRandom("Aalsmeer", "Naaldwijk", "Rijnsburg", "Boskoop")) 
            .RuleFor(
                c => c.HouseNumber, 
                  f => f.Address.BuildingNumber()) 
            .RuleFor(
                c => c.HouseNumberSuffix, 
                  f => "") 
            .RuleFor(
                c => c.PostalCode, 
                  f => f.Address.ZipCode( "???? ##")) 
            .RuleFor(
                c => c.CountryCode, "NL");
    }
}