using Bogus;
using LeafBidAPI.Data.factories;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace LeafBidAPI.Data.seeders;

public class AuctionSeeder(ApplicationDbContext dbContext, UserManager<User> userManager) : ISeeder
{
    public async Task SeedAsync()
    {
        // Seed auctions
        Auction[] auctions = new AuctionFactory(dbContext, userManager).Generate(16);
        await dbContext.AddRangeAsync(auctions);
        await dbContext.SaveChangesAsync();

        auctions = dbContext.Auctions.ToArray();
        foreach (Auction auction in auctions)
        {
            // Seed auction products
            AuctionProduct[] auctionProducts = new AuctionProductFactory(dbContext, auction).Generate(10);
            await dbContext.AddRangeAsync(auctionProducts);
            await dbContext.SaveChangesAsync();
        }
        
        // Seed auction sales
        AuctionSale[] auctionSales;
        
        try
        {
            auctionSales = new AuctionSaleFactory(dbContext, userManager).Generate(8);
            await dbContext.AddRangeAsync(auctionSales);
            await dbContext.SaveChangesAsync();
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        
        auctionSales = dbContext.AuctionSales.ToArray();
        foreach (AuctionSale auctionSale in auctionSales)
        {
            // Seed auction sale products
            try
            {
                AuctionSaleProduct[] auctionSaleProducts =
                    new AuctionSaleProductFactory(dbContext, auctionSale).Generate(20);
                await dbContext.AddRangeAsync(auctionSaleProducts);
                await dbContext.SaveChangesAsync();
            }
            catch (NotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}