using LeafBidAPI.Data;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;
using LeafBidAPI.Services;
using LeafBidAPITest.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LeafBidAPITest.Services;

public class AuctionServiceTest
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    private readonly Mock<UserManager<User>> _userManagerMock = DummyUsers.CreateUserManagerMock();

    [Fact]
    public async Task GetAuctions_ReturnsAllAuctions_Successfully()
    {
        // Arrange
        await using ApplicationDbContext context = new(_dbOptions);
        List<Auction> auctionList = DummyAuctions.GetFakeAuctions();
        context.Auctions.AddRange(auctionList);
        await context.SaveChangesAsync();

        AuctionService service = new(context, _userManagerMock.Object);

        // Act
        List<Auction> result = await service.GetAuctions();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(3, result[2].Id);
        Assert.Equal(4, result[3].Id);
    }

    [Fact]
    public async Task GetAuctionById_ThrowsNotFound_WhenAuctionDoesNotExist()
    {
        // Arrange
        await using ApplicationDbContext context = new(_dbOptions);
        List<Auction> fakeAuctions = DummyAuctions.GetFakeAuctions();
        context.Auctions.AddRange(fakeAuctions);
        await context.SaveChangesAsync();

        AuctionService service = new(context, _userManagerMock.Object);
        const int nonExistingId = 5;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAuctionById(nonExistingId));
    }

    [Fact]
    public async Task GetAuctionById_ReturnsAuction_WhenAuctionExists()
    {
        // Arrange
        await using ApplicationDbContext context = new(_dbOptions);
        List<Auction> fakeAuctions = DummyAuctions.GetFakeAuctions();
        context.Auctions.AddRange(fakeAuctions);
        await context.SaveChangesAsync();

        AuctionService service = new(context, _userManagerMock.Object);

        const int existingId = 2;
        Auction expectedAuction = fakeAuctions.First(a => a.Id == existingId);

        // Act
        Auction result = await service.GetAuctionById(existingId);

        // Assert
        Assert.Equal(expectedAuction.Id, result.Id);
        Assert.Equal(expectedAuction.UserId, result.UserId);
        Assert.Equal(expectedAuction.IsLive, result.IsLive);
    }
}
