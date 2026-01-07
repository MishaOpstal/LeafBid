using Bogus;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.factories;

public class AuctionSaleProductFactory(
    ApplicationDbContext dbContext,
    AuctionSale auctionSale
) : Factory<AuctionSaleProduct>
{
    protected override Faker<AuctionSaleProduct> BuildRules()
    {
        Faker faker = new();
        
        List<AuctionProduct> auctionProductsForAuction = dbContext.AuctionProducts
            .Where(ap => ap.AuctionId == auctionSale.AuctionId)
            .Include(ap => ap.RegisteredProduct)
            .ToList();

        if (auctionProductsForAuction.Count == 0)
        {
            throw new NotFoundException(
                $"No auction products were found for AuctionId {auctionSale.AuctionId}"
            );
        }

        List<(int ProductId, int RemainingQuantity, decimal MinPrice, decimal MaxPrice)> viable = new();

        foreach (AuctionProduct auctionProduct in auctionProductsForAuction)
        {
            RegisteredProduct? registeredProduct = auctionProduct.RegisteredProduct;
            if (registeredProduct == null)
            {
                continue;
            }

            if (registeredProduct.ProductId == 0)
            {
                continue;
            }

            if (!registeredProduct.MaxPrice.HasValue)
            {
                continue;
            }

            int registeredProductId = registeredProduct.Id;

            int alreadySold = dbContext.AuctionSaleProducts
                .Where(asp =>
                    asp.AuctionSaleId == auctionSale.Id &&
                    asp.RegisteredProductId == registeredProductId
                )
                .Sum(asp => (int?)asp.Quantity) ?? 0;

            int remainingQuantity = auctionProduct.AuctionStock - alreadySold;
            if (remainingQuantity <= 0)
            {
                continue;
            }

            decimal minPrice = registeredProduct.MinPrice;
            decimal maxPrice = registeredProduct.MaxPrice.Value;

            if (maxPrice < minPrice)
            {
                (minPrice, maxPrice) = (maxPrice, minPrice);
            }

            viable.Add((registeredProductId, remainingQuantity, minPrice, maxPrice));
        }

        if (viable.Count == 0)
        {
            throw new OutOfStockException(
                $"No viable products with remaining stock and MaxPrice were found " +
                $"for AuctionSaleId {auctionSale.Id} (AuctionId {auctionSale.AuctionId})"
            );
        }

        (int usedRegisteredProductId, int remainingQuantityFinal, decimal minPriceFinal, decimal maxPriceFinal)
            = faker.Random.ListItem(viable);

        return new Faker<AuctionSaleProduct>()
            .RuleFor(asp => asp.AuctionSaleId, auctionSale.Id)
            .RuleFor(asp => asp.RegisteredProductId, usedRegisteredProductId)
            .RuleFor(asp => asp.Quantity, f => f.Random.Int(1, remainingQuantityFinal))
            .RuleFor(asp => asp.Price, f =>
            {
                if (maxPriceFinal <= minPriceFinal)
                {
                    return minPriceFinal;
                }

                int discountPercent = f.Random.Int(1, 75);
                decimal discounted = maxPriceFinal * (100m - discountPercent) / 100m;

                if (discounted < minPriceFinal)
                {
                    discounted = minPriceFinal;
                }

                return decimal.Round(discounted, 2, MidpointRounding.AwayFromZero);
            });
    }
}
