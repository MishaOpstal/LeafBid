using Bogus;
using LeafBidAPI.Models;

namespace LeafBidAPI.Data.factories;

public class ProductFactory : Factory<Product>
{
    protected override Faker<Product> BuildRules()
    {
        return new Faker<Product>()
            .RuleFor(p => p.Species, f => f.Commerce.Categories(1)[0])
            .RuleFor(
                p => p.Name,
                (f, p) => $"{p.Species} {f.Commerce.ProductMaterial()}"
            )
            .RuleFor(
                p => p.Description,
                f => f.Commerce.ProductDescription()
            )
            .RuleFor(
                p => p.Picture,
                (f, p) => f.Image.PlaceholderUrl(1200, 1200, p.Name)
            );
    }
}