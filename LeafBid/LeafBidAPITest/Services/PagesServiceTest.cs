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

        // 1. Create and save initial data to get generated IDs
        var auctions = DummyAuctions.GetFakeAuctions();
        var products = DummyProducts.GetFakeProducts();
        var registeredProducts = DummyRegisteredProducts.GetFakeRegisteredProducts();

        foreach (var a in auctions)
        {
            a.IsVisible = true;
        }

        var companies = new List<Company>
        {
            new()
            {
                Id = 1, Name = "Company 1", Street = "Street 1", City = "City 1", HouseNumber = "1",
                PostalCode = "1234AB", CountryCode = "NL"
            },
            new()
            {
                Id = 2, Name = "Company 2", Street = "Street 2", City = "City 2", HouseNumber = "2",
                PostalCode = "1234AB", CountryCode = "NL"
            },
            new()
            {
                Id = 3, Name = "Company 3", Street = "Street 3", City = "City 3", HouseNumber = "3",
                PostalCode = "1234AB", CountryCode = "NL"
            },
            new()
            {
                Id = 4, Name = "Company 4", Street = "Street 4", City = "City 4", HouseNumber = "4",
                PostalCode = "1234AB", CountryCode = "NL"
            }
        };
        var users = new List<User>
        {
            new() { Id = "user1", UserName = "user1", Email = "user1@test.com" },
            new() { Id = "user2", UserName = "user2", Email = "user2@test.com" },
            new() { Id = "user3", UserName = "user3", Email = "user3@test.com" },
            new() { Id = "user4", UserName = "user4", Email = "user4@test.com" }
        };

        context.Companies.AddRange(companies);
        context.Users.AddRange(users);
        context.Auctions.AddRange(auctions);
        context.Products.AddRange(products);
        context.RegisteredProducts.AddRange(registeredProducts);
        await context.SaveChangesAsync();

        // 2. Create AuctionProducts with the generated IDs
        List<AuctionProduct> auctionProducts = [];
        foreach (Auction auction in auctions)
        {
            auctionProducts.AddRange(
                registeredProducts.Select((t, j) =>
                    new AuctionProduct
                    {
                        AuctionId = auction.Id,
                        RegisteredProductId = t.Id,
                        ServeOrder = j + 1
                    }
                )
            );
        }

        context.AuctionProducts.AddRange(auctionProducts);
        await context.SaveChangesAsync();

        PagesServices pagesServices = new PagesServices(context, _userManagerMock.Object);

        // Act 
        var result = await pagesServices.GetAuctionsWithProductsPerClockLocation();

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetAuctionWithProductsById_ReturnsProductsNotFoundException()
    {
        // Arrange (create auction without products)
        await using ApplicationDbContext context = new(_dbOptions);
        List<Auction> auctionList = DummyAuctions.GetFakeAuctions();
        foreach (var a in auctionList)
        {
            a.IsVisible = true;
        }

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
        // Arrange
        await using ApplicationDbContext context = new(_dbOptions);

        // 1. Create and save initial data to get generated IDs
        var auctions = DummyAuctions.GetFakeAuctions();
        var products = DummyProducts.GetFakeProducts();
        var registeredProducts = DummyRegisteredProducts.GetFakeRegisteredProducts();

        foreach (var a in auctions)
        {
            a.IsVisible = true;
        }

        foreach (var rp in registeredProducts)
        {
            rp.Stock = 100; // Ensure stock
        }

        var companies = new List<Company>
        {
            new()
            {
                Id = 1, Name = "Company 1", Street = "Street 1", City = "City 1", HouseNumber = "1",
                PostalCode = "1234AB", CountryCode = "NL"
            },
            new()
            {
                Id = 2, Name = "Company 2", Street = "Street 2", City = "City 2", HouseNumber = "2",
                PostalCode = "1234AB", CountryCode = "NL"
            },
            new()
            {
                Id = 3, Name = "Company 3", Street = "Street 3", City = "City 3", HouseNumber = "3",
                PostalCode = "1234AB", CountryCode = "NL"
            },
            new()
            {
                Id = 4, Name = "Company 4", Street = "Street 4", City = "City 4", HouseNumber = "4",
                PostalCode = "1234AB", CountryCode = "NL"
            }
        };
        var users = new List<User>
        {
            new() { Id = "user1", UserName = "user1", Email = "user1@test.com" },
            new() { Id = "user2", UserName = "user2", Email = "user2@test.com" },
            new() { Id = "user3", UserName = "user3", Email = "user3@test.com" },
            new() { Id = "user4", UserName = "user4", Email = "user4@test.com" }
        };

        context.Companies.AddRange(companies);
        context.Users.AddRange(users);
        context.Auctions.AddRange(auctions);
        context.Products.AddRange(products);
        context.RegisteredProducts.AddRange(registeredProducts);
        await context.SaveChangesAsync();

        // 2. Create AuctionProducts with the generated IDs
        List<AuctionProduct> auctionProducts = [];
        foreach (Auction auction in auctions)
        {
            auctionProducts.AddRange(
                registeredProducts.Select((t, j) =>
                    new AuctionProduct
                    {
                        AuctionId = auction.Id,
                        RegisteredProductId = t.Id,
                        ServeOrder = j + 1
                    }
                )
            );
        }

        context.AuctionProducts.AddRange(auctionProducts);
        await context.SaveChangesAsync();

        PagesServices pagesServices = new PagesServices(context, _userManagerMock.Object);

        // Act 
        var result = await pagesServices.GetAuctionsWithProductsPerClockLocation();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, auctionDto =>
        {
            Assert.NotNull(auctionDto.RegisteredProducts);
            Assert.NotEmpty(auctionDto.RegisteredProducts);
        });
    }
}