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
                        RegisteredProduct = rp
                    })
                    .AsEnumerable()
                    .Select(x => (x.RegisteredProduct, AvailableStock: x.RegisteredProduct.Stock))
                    .Where(x => x.AvailableStock > 0 && !_usedRegisteredProductIds.Contains(x.RegisteredProduct.Id))
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
                };
            });
    }
}
