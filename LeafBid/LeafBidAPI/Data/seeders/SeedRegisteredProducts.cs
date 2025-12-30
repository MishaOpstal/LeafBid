using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class SeedRegisteredProducts
{
    public static async Task SeedRegisteredProductsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var providerId = await context.Users
            .Where(u => u.UserName == "Provider")
            .Select(u => u.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        await SeedRegisteredProductAsync(context, providerId, "TestProduct1", 2.50m, 3.50m, "Netherlands", DateTime.UtcNow.AddDays(-2), 19, null, 100, cancellationToken);
        await SeedRegisteredProductAsync(context, providerId, "TestProduct2", 1.50m, 2.50m, "Netherlands", DateTime.UtcNow.AddDays(-1), null, 15, 50, cancellationToken);
        await SeedRegisteredProductAsync(context, providerId, "TestProduct3", 10.00m, 12.00m, "Thailand", DateTime.UtcNow.AddDays(-5), null, 12, 30, cancellationToken);
        await SeedRegisteredProductAsync(context, providerId, "TestProduct4", 2.50m, 3.00m, "Spain", DateTime.UtcNow.AddDays(-3), 22, null, 80, cancellationToken);
    }

    private static async Task SeedRegisteredProductAsync(
        ApplicationDbContext context,
        string userId,
        string productName,
        decimal minPrice,
        decimal maxPrice,
        string region,
        DateTime harvestedAt,
        int? stemLength,
        int? potSize,
        int stock,
        CancellationToken cancellationToken)
    {
        var productId = await context.Products
            .Where(p => p.Name == productName)
            .Select(p => p.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        if (await context.RegisteredProducts.AnyAsync(rp => rp.UserId == userId && rp.ProductId == productId, cancellationToken))
            return;

        context.RegisteredProducts.Add(new RegisteredProduct
        {
            UserId = userId,
            ProductId = productId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Region = region,
            HarvestedAt = harvestedAt,
            StemLength = stemLength,
            PotSize = potSize,
            Stock = stock
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}