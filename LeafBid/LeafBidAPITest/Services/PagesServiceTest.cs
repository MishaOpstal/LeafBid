using LeafBidAPI.Data;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;
using LeafBidAPI.Services;
using LeafBidAPITest.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LeafBidAPITest.Services;

public class PagesServiceTest
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    private readonly Mock<UserManager<User>> _userManagerMock = DummyUsers.CreateUserManagerMock();
    
    [Fact]
    public async Task GetAuctionPerActiveClockLocation_ReturnsAuctionWithProductsDto_Successfully()
    {
        // Arrange (create auction and products)
        await using ApplicationDbContext context = new(_dbOptions);
        List<Auction> auctionList = DummyAuctions.GetFakeAuctions();
        context.Auctions.AddRange(auctionList);
        await context.SaveChangesAsync();
        
        List<AuctionProduct> productList = DummyAuctionProducts.GetFakeAuctionProducts();
        context.AuctionProducts.AddRange(productList);
        await context.SaveChangesAsync();

        List<RegisteredProduct> registeredProducts = DummyRegisteredProducts.GetFakeRegisteredProducts();
        context.RegisteredProducts.AddRange(registeredProducts);
        await context.SaveChangesAsync();

        List<Product> products = DummyProducts.GetFakeProducts();
        context.Products.AddRange(products);
        await context.SaveChangesAsync();
        
        PagesServices pagesServices = new PagesServices(context, _userManagerMock.Object);
        
        // Act 
        var result =  await pagesServices.GetAuctionPerActiveClockLocation();
        
        // Assert
        Assert.NotEmpty(result.Auctions);
        foreach (var auction in result.Auctions)
        {
            Assert.NotNull(auction.RegisteredProducts);
            Assert.NotEmpty(auction.RegisteredProducts);
        }
    }
}