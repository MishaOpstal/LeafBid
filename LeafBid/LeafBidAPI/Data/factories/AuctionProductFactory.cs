using Bogus;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.factories;

public class AuctionProductFactory(ApplicationDbContext dbContext, Auction auction)
    : Factory<AuctionProduct>
{
    private readonly HashSet<int> _usedRegisteredProductIds =
        dbContext.AuctionProducts
            .Where(ap => ap.AuctionId == auction.Id)
            .Select(ap => ap.RegisteredProductId)
            .ToHashSet();

    protected override Faker<AuctionProduct> BuildRules()
    {
        return new Faker<AuctionProduct>()
            .CustomInstantiator(f =>
            {
                // Build viable list based on current used set
                List<(RegisteredProduct Product, int AvailableStock)> viable = dbContext.RegisteredProducts
                    .Select(rp => new
                    {
                        Product = rp,
                        UsedStock = dbContext.AuctionProducts
                            .Where(ap => ap.RegisteredProductId == rp.Id)
                            .Sum(ap => (int?)ap.AuctionStock) ?? 0
                    })
                    .AsEnumerable()
                    .Select(x => (x.Product, AvailableStock: x.Product.Stock - x.UsedStock))
                    .Where(x => x.AvailableStock > 0 && !_usedRegisteredProductIds.Contains(x.Product.Id))
                    .ToList();

                if (viable.Count == 0)
                {
                    throw new OutOfStockException("No viable registered products for this auction");
                }

                (RegisteredProduct chosen, int availableStock) = f.Random.ListItem(viable);

                _usedRegisteredProductIds.Add(chosen.Id);
                int serveOrder = _usedRegisteredProductIds.Count;

                return new AuctionProduct
                {
                    AuctionId = auction.Id,
                    RegisteredProductId = chosen.Id,
                    ServeOrder = serveOrder,
                    AuctionStock = f.Random.Int(1, availableStock)
                };
            });
    }
}
