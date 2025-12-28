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
        
        //seed registered products
        await SeedRegisteredProducts.seedRegisteredProductsAsync(context, CancellationToken.None);
        
        // seed auctions
        await SeedAuctions.SeedAuctionsAsync(context, CancellationToken.None);
        
        // seed auction sales
        await SeedAuctionSales.SeedAuctionSalesAsync(context, CancellationToken.None);
        
        // seed auction sale products
        await SeedAuctionSaleProducts.SeedAuctionSaleProductsAsync(context, CancellationToken.None);
    }
}