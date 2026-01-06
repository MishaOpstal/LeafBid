using LeafBidAPI.Data;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;
using LeafBidAPI.Services;
using LeafBidAPITest.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LeafBidAPITest.Services;

public class PagesServiceTest
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions =
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private readonly Mock<UserManager<User>> _userManagerMock = DummyUsers.CreateUserManagerMock();

    [Fact]
    public async Task GetAuctionPerActiveClockLocation_ReturnsAuctionWithProductsDto_Successfully()
    {
        // Arrange
        await using ApplicationDbContext context = new(_dbOptions);

        // 1. Create Data
        var auctions = DummyAuctions.GetFakeAuctions();
        var products = DummyProducts.GetFakeProducts();
        var registeredProducts = DummyRegisteredProducts.GetFakeRegisteredProducts();
        var auctionProducts = DummyAuctionProducts.GetFakeAuctionProducts();

        // 2. IMPORTANT: Adjust dates to match the hardcoded date in PagesServices (2019-12-29)
        foreach (var a in auctions)
        {
            a.StartDate = new DateTime(2019, 12, 29);
        }

        // 3. Link objects manually for InMemoryDatabase to work with Include/ThenInclude
        foreach (var rp in registeredProducts)
        {
            rp.Product = products.FirstOrDefault(p => p.Id == rp.ProductId);
        }

        foreach (var ap in auctionProducts)
        {
            ap.RegisteredProduct = registeredProducts.FirstOrDefault(rp => rp.Id == ap.RegisteredProductId);
        }

        // 4. Add to context
        context.Auctions.AddRange(auctions);
        context.Products.AddRange(products);
        context.RegisteredProducts.AddRange(registeredProducts);
        context.AuctionProducts.AddRange(auctionProducts);
        await context.SaveChangesAsync();

        PagesServices pagesServices = new PagesServices(context, _userManagerMock.Object);

        // Act 
        var result = await pagesServices.GetAuctionPerActiveClockLocation();

        // Assert
        Assert.NotEmpty(result);

    }

    [Fact]
    public async Task GetAuctionWithProductsById_ReturnsProductsNotFoundException()
    {
        // Arrange (create auction without products)
        await using ApplicationDbContext context = new(_dbOptions);
        List<Auction> auctionList = DummyAuctions.GetFakeAuctions();
        context.Auctions.AddRange(auctionList);
        await context.SaveChangesAsync();

        PagesServices pagesServices = new PagesServices(context, _userManagerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await pagesServices.GetAuctionWithProductsById(auctionList.First().Id);
        });
    }

    [Fact]
    public async Task GetAuctionPerActiveClockLocationReturnsProducts_OK()
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
        var result = await pagesServices.GetAuctionPerActiveClockLocation();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, auctionDto =>
        {
            Assert.NotNull(auctionDto.RegisteredProducts);
            Assert.NotEmpty(auctionDto.RegisteredProducts);
        });
    }
}