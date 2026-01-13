using System;
using LeafBidAPI.Models;
using LeafBidAPI.Configuration;
using Microsoft.Extensions.Options;

namespace LeafBidAPI.Helpers;

public sealed class AuctionHelper(IOptions<AuctionTimerSettings> options)
{
    private readonly AuctionTimerSettings _settings = options.Value;

    public double GetProductDurationSeconds(RegisteredProduct product)
    {
        decimal startPrice = product.MaxPrice ?? product.MinPrice;
        decimal minPrice = product.MinPrice;
        decimal rangeCents = (startPrice - minPrice) * 100;

        decimal startCents = startPrice * 100;
        decimal baseDecreaseCentsPerSecond = startCents * 0.05m;

        if (baseDecreaseCentsPerSecond <= 0)
        {
            return 0;
        }

        double impliedSeconds = (double)(rangeCents / baseDecreaseCentsPerSecond);
        double durationSeconds = Math.Max(_settings.MinDurationForAuctionTimer, impliedSeconds);

        if (_settings.UseMaxDurationForAuctionTimer)
        {
            durationSeconds = Math.Min(
                durationSeconds,
                _settings.MaxDurationForAuctionTimer
            );
        }

        return durationSeconds;
    }
}