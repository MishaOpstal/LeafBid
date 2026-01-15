using Bogus;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class RegisteredProductSeeder(
    ApplicationDbContext appDbContext,
    UserManager<User> userManager
) : ISeeder
{
    public async Task SeedAsync()
    {
        Faker faker = new();

        User[] providers = (await userManager.GetUsersInRoleAsync("Provider")).ToArray();
        if (providers.Length == 0)
        {
            return;
        }

        Product[] products = await appDbContext.Products.ToArrayAsync();
        if (products.Length == 0)
        {
            return;
        }

        List<RegisteredProduct> toInsert = new();

        foreach (User user in providers)
        {
            if (user.CompanyId == null)
            {
                continue;
            }

            int productCount = faker.Random.Int(0, 10);

            for (int i = 0; i < productCount; i++)
            {
                Product chosenProduct = faker.Random.ArrayElement(products);

                decimal minPrice = faker.Random.Decimal(0.25M, 10);
                decimal maxPrice = faker.Random.Decimal(minPrice, 50);
                bool usePotSize = faker.Random.Bool();
                double length = faker.Random.Double(0, 100);

                RegisteredProduct registeredProduct = new()
                {
                    UserId = user.Id,
                    ProductId = chosenProduct.Id,
                    CompanyId = user.CompanyId.Value,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Stock = faker.Random.Int(10, 1000),
                    Region = faker.Address.County(),
                    HarvestedAt = faker.Date.Recent()
                };

                if (usePotSize)
                {
                    registeredProduct.PotSize = length;
                    registeredProduct.StemLength = null;
                }
                else
                {
                    registeredProduct.StemLength = length;
                    registeredProduct.PotSize = null;
                }

                toInsert.Add(registeredProduct);
            }
        }

        if (toInsert.Count == 0)
        {
            return;
        }

        await appDbContext.RegisteredProducts.AddRangeAsync(toInsert);
        await appDbContext.SaveChangesAsync();
    }
}
