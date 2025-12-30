using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedProducts
{
    public static async Task SeedProductsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        await SeedProductAsync(context, "TestProduct1", "Classic red rose", "Rosa chinensis", cancellationToken);
        await SeedProductAsync(context, "TestProduct2", "Yellow spring tulip", "Tulipa gesneriana", cancellationToken);
        await SeedProductAsync(context, "TestProduct3", "Purple lily", "Lilium pratense", cancellationToken);
        await SeedProductAsync(context, "TestProduct4", "White lily", "Lilium occidentale", cancellationToken);
    }

    private static async Task SeedProductAsync(
        ApplicationDbContext context,
        string name,
        string description,
        string species,
        CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(p => p.Name == name, cancellationToken))
            return;

        context.Products.Add(new Product
        {
            Name = name,
            Description = description,
            Species = species,
            Picture = null
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}