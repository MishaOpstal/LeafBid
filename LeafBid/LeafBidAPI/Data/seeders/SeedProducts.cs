using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedProducts
{
    public static async Task SeedProductsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(cancellationToken))
            return;

        context.Products.AddRange(
            new Product
            {
                Name = "TestProduct1",
                Description = "Classic red rose",
                Species = "Rosa chinensis",
                Picture = null
            },
            new Product
            {
                Name = "TestProduct2",
                Description = "Yellow spring tulip",
                Species = "Tulipa gesneriana",
                Picture = null
            },
            new Product
            {
                Name = "TestProduct3",
                Description = "Purple lily",
                Species = "Lilium pratense",
                Picture = null
            },
            new Product
            {
                Name = "TestProduct4",
                Description = "White lily",
                Species = "Lilium occidentale",
                Picture = null
            }
        );

        await context.SaveChangesAsync(cancellationToken);
    }
}