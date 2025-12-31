using LeafBidAPI.Data.factories;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class ProductSeeder(ApplicationDbContext appDbContext) : ISeeder
{
    public async Task SeedAsync()
    {
        bool hasProducts = await appDbContext.Products.AnyAsync();
        if (hasProducts)
        {
            return;
        }

        Product[] products = new ProductFactory().Generate(100);
        await appDbContext.AddRangeAsync(products);
        await appDbContext.SaveChangesAsync();
    }
}