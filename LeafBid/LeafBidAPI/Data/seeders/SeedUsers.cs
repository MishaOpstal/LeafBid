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
            email: "Buyer@Leafbid.com",
            userName: "Buyer",
            password: "Buyer123!?",
            role: "Buyer");

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
        string role)
    {
        if (await userManager.FindByEmailAsync(email) != null)
            return;

        User user = new()
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}