using LeafBidAPI.Models;

namespace LeafBidAPITest.Helpers;

public class DummyAuctions
{
    
    public static List<Auction> GetFakeAuctions()
    {
        return new List<Auction>
        {
            new()
            {
                Id = 1,
                StartDate = DateTime.UtcNow,
                IsLive = false,
                IsVisible = true,
                UserId = "user1",
                ClockLocationEnum = LeafBidAPI.Enums.ClockLocationEnum.Aalsmeer
            },
            new()
            {
                Id = 2,
                StartDate = DateTime.UtcNow.AddHours(1),
                IsLive = true,
                IsVisible = true,
                UserId = "user2",
                ClockLocationEnum = LeafBidAPI.Enums.ClockLocationEnum.Eelde
            },
            new()
            {
                Id = 3,
                StartDate = DateTime.UtcNow.AddHours(2),
                IsLive = false,
                IsVisible = false,
                UserId = "user3",
                ClockLocationEnum = LeafBidAPI.Enums.ClockLocationEnum.Rijnsburg
            },
            new()
            {
                Id = 4,
                StartDate = DateTime.UtcNow.AddHours(3),
                IsLive = true,
                IsVisible = true,
                UserId = "user4",
                ClockLocationEnum = LeafBidAPI.Enums.ClockLocationEnum.Naaldwijk
            }
        };
    }
}