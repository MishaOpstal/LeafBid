using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace LeafBidAPI.Data.seeders;

public class SeedUsers
{
    public static async Task SeedUsersWithRolesAsync(
        UserManager<User> userManager)
    {
        await SeedUserAsync(
            userManager,
            email: "Auctioneer@Leafbid.com",
            userName: "Auctioneer",
            password: "Auctioneer123!?",
            role: "Auctioneer");

        await SeedUserAsync(
            userManager,
            email: "Provider@Leafbid.com",
            userName: "Provider",
            password: "Provider123!?",
            role: "Provider");

        await SeedUserAsync(
            userManager,
            email: "Buyer1@Leafbid.com",
            userName: "Buyer1",
            password: "Buyer123!?",
            role: "Buyer",
            companyId: 1);
        
        await SeedUserAsync(
            userManager,
            email: "Buyer2@Leafbid.com",
            userName: "Buyer2",
            password: "Buyer123!?",
            role: "Buyer",
            companyId: 2);
        
        await SeedUserAsync(
            userManager,
            email: "Buyer3@Leafbid.com",
            userName: "Buyer3",
            password: "Buyer123!?",
            role: "Buyer",
            companyId: 3);
        
        await SeedUserAsync(
            userManager,
            email: "Buyer4@Leafbid.com",
            userName: "Buyer4",
            password: "Buyer123!?",
            role: "Buyer",
            companyId: 1);
        
        await SeedUserAsync(
            userManager,
            email: "Buyer5@Leafbid.com",
            userName: "Buyer5",
            password: "Buyer123!?",
            role: "Buyer",
            companyId: 1);

        await SeedUserAsync(
            userManager,
            email: "Admin@Leafbid.com",
            userName: "Admin",
            password: "Admin123!?",
            role: "Admin");
    }

    private static async Task SeedUserAsync(
        UserManager<User> userManager,
        string email,
        string userName,
        string password,
        string role,
        int?  companyId = null)
    {
        if (await userManager.FindByEmailAsync(email) != null)
            return;

        User user = new()
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            CompanyId = companyId
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}