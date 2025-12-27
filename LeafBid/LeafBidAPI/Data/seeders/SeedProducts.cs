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
                Name = "Rose",
                Description = "Classic red rose",
                Species = "Rosa chinensis",
                Picture = null
            },
            new Product
            {
                Name = "Tulip",
                Description = "Yellow spring tulip",
                Species = "Tulipa gesneriana",
                Picture = null
            }
        );

        await context.SaveChangesAsync(cancellationToken);
    }
}