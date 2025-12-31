using LeafBidAPI.Data.factories;
using LeafBidAPI.Models;

namespace LeafBidAPI.Data.seeders;

public class CompanySeeder(ApplicationDbContext dbContext) : ISeeder
{
    public async Task SeedAsync()
    {
        Company[] companies = new CompanyFactory().Generate(10);
        await dbContext.AddRangeAsync(companies);
        await dbContext.SaveChangesAsync();
    }
}