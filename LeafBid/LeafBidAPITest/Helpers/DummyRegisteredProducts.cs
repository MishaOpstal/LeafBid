using LeafBidAPI.Models;

namespace LeafBidAPITest.Helpers;

public class DummyRegisteredProducts
{
    public static List<RegisteredProduct> GetFakeRegisteredProducts()
    {
        return
        [
            new RegisteredProduct
            {
                Id = 1,
                ProductId = 1,
                MinPrice = 1.34m,
                MaxPrice = 1.34m,
                Region = "Netherlands",
                PotSize = null,
                StemLength = 19,
                Stock = 50,
                HarvestedAt = DateTime.UtcNow.AddDays(-2),
                UserId = "user1"
            },

            new RegisteredProduct
            {
                Id = 2,
                ProductId = 2,
                MinPrice = 0.89m,
                MaxPrice = 0.89m,
                Region = "Netherlands",
                PotSize = null,
                StemLength = 17,
                Stock = 100,
                HarvestedAt = DateTime.UtcNow.AddDays(-1),
                UserId = "user1"
            },

            new RegisteredProduct
            {
                Id = 3,
                ProductId = 3,
                MinPrice = 15.00m,
                MaxPrice = 15.00m,
                Region = "Thailand",
                PotSize = 12,
                StemLength = null,
                Stock = 30,
                HarvestedAt = DateTime.UtcNow.AddDays(-5),
                UserId = "user1"
            },

            new RegisteredProduct
            {
                Id = 4,
                ProductId = 4,
                MinPrice = 2.50m,
                MaxPrice = 2.50m,
                Region = "Spain",
                PotSize = null,
                StemLength = 22,
                Stock = 75,
                HarvestedAt = DateTime.UtcNow.AddDays(-3),
                UserId = "user1"
            }
        ];
    }
}