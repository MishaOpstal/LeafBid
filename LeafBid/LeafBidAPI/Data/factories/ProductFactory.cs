using Bogus;
using LeafBidAPI.Models;

namespace LeafBidAPI.Data.factories;

public class ProductFactory : Factory<Product>
{
    protected override Faker<Product> BuildRules()
    {
        Faker faker = new();

        string productName = faker.Commerce.ProductName();
        
        return new Faker<Product>()
            .RuleFor(
                p => p.Name,
                productName
            )
            .RuleFor(
                p => p.Description,
                f => f.Commerce.ProductDescription()
            )
            .RuleFor(
                p => p.Species,
                f => f.Random.Word()
            )
            .RuleFor(
                p => p.Picture,
                f => f.Image.PlaceholderUrl(1200, 1200, productName)
            );
    }
}