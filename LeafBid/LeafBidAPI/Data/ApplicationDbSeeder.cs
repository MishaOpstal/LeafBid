using LeafBidAPI.Data.seeders;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace LeafBidAPI.Data;

public class ApplicationDbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        IServiceProvider serviceProvider = scope.ServiceProvider;
        
        ApplicationDbContext context = serviceProvider
            .GetRequiredService<ApplicationDbContext>();
        
        UserManager<User> userManager = serviceProvider
            .GetRequiredService<UserManager<User>>();

        
        // seed users
        await SeedUsers.SeedUsersWithRolesAsync(userManager);
        
        // seed products
        await SeedProducts.SeedProductsAsync(context, CancellationToken.None);
    }
}