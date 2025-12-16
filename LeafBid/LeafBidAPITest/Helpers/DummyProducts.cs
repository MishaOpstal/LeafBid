using LeafBidAPI.Models;

namespace LeafBidAPITest.Helpers;

public class DummyProducts
{
    public static List<Product> GetFakeProducts()
    {
        return
        [
            new Product
            {
                Id = 1,
                Name = "Rose Bouquet",
                Description = "A beautiful bouquet of red roses.",
                Species = "Rosa"
            },

            new Product
            {
                Id = 2,
                Name = "Tulip Bunch",
                Description = "A vibrant bunch of tulips in various colors.",
                Species = "Tulipa"
            },

            new Product
            {
                Id = 3,
                Name = "Potted Orchid",
                Description = "A delicate potted orchid plant.",
                Species = "Orchidaceae"
            },

            new Product
            {
                Id = 4,
                Name = "Sunflower Bundle",
                Description = "A cheerful bundle of sunflowers.",
                Species = "Helianthus"
            }
        ];
    }
}