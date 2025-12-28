using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedRegisteredProducts
{
    public static async Task seedRegisteredProductsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.RegisteredProducts.AnyAsync(cancellationToken))
            return; 
        
        context.RegisteredProducts.AddRange(
            new RegisteredProduct
            {
                UserId = await context.Users.Where(u => u.UserName == "Provider").Select(u => u.Id).FirstAsync(cancellationToken: cancellationToken),
                ProductId = await context.Products.Where(p => p.Name == "TestProduct1").Select(p => p.Id).FirstAsync(cancellationToken: cancellationToken),
                MinPrice = 2.50m,
                MaxPrice = 3.50m,
                Region = "Netherlands",
                HarvestedAt = DateTime.UtcNow.AddDays(-2),
                StemLength = 19,
                Stock = 100
            },
            new RegisteredProduct
            {
                UserId = await context.Users.Where(u => u.UserName == "Provider").Select(u => u.Id).FirstAsync(cancellationToken: cancellationToken),
                ProductId = await context.Products.Where(p => p.Name == "TestProduct2").Select(p => p.Id).FirstAsync(cancellationToken: cancellationToken),
                MinPrice = 1.50m,
                MaxPrice = 2.50m,
                Region = "Netherlands",
                HarvestedAt = DateTime.UtcNow.AddDays(-1),
                PotSize = 15,
                Stock = 50
            },
            new RegisteredProduct
            {
                UserId = await context.Users.Where(u => u.UserName == "Provider").Select(u => u.Id).FirstAsync(cancellationToken: cancellationToken),
                ProductId = await context.Products.Where(p => p.Name == "TestProduct3").Select(p => p.Id).FirstAsync(cancellationToken: cancellationToken),
                MinPrice = 10.00m,
                MaxPrice = 12.00m,
                Region = "Thailand",
                HarvestedAt = DateTime.UtcNow.AddDays(-5),
                PotSize = 12,
                Stock = 30
            },
            new RegisteredProduct
            {
                UserId = await context.Users.Where(u => u.UserName == "Provider").Select(u => u.Id).FirstAsync(cancellationToken: cancellationToken),
                ProductId = await context.Products.Where(p => p.Name == "TestProduct4").Select(p => p.Id).FirstAsync(cancellationToken: cancellationToken),
                MinPrice = 2.50m,
                MaxPrice = 3.00m,
                Region = "Spain",
                HarvestedAt = DateTime.UtcNow.AddDays(-3),
                StemLength = 22,
                Stock = 80
            }
        );
    }
}