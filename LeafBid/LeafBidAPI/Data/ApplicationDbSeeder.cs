using LeafBidAPI.Data.seeders;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace LeafBidAPI.Data;

public class ApplicationDbSeeder
{
    public static Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        IServiceProvider serviceProvider = scope.ServiceProvider;
        
        ApplicationDbContext context = serviceProvider
            .GetRequiredService<ApplicationDbContext>();
        
        UserManager<User> userManager = serviceProvider
            .GetRequiredService<UserManager<User>>();

        
        // seed copmpanies
        SeedCompanies.SeedCompaniesAsync(context, CancellationToken.None);
        
        // seed users
        SeedUsers.SeedUsersWithRolesAsync(userManager);
        
        // seed products
        SeedProducts.SeedProductsAsync(context, CancellationToken.None);
        
        //seed registered products
        SeedRegisteredProducts.SeedRegisteredProductsAsync(context, CancellationToken.None);
        
        // seed auctions
        SeedAuctions.SeedAuctionsAsync(context, CancellationToken.None);
        
        // seed auction products
        SeedAuctionProducts.SeedAuctionProductsAsync(context, CancellationToken.None);
        
        // seed auction sales
        SeedAuctionSales.SeedAuctionSalesAsync(context, CancellationToken.None);
        
        // seed auction sale products
        SeedAuctionSaleProducts.SeedAuctionSaleProductsAsync(context, CancellationToken.None);
        
        return Task.CompletedTask;
    }
}