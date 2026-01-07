using System;
using LeafBidAPI.Models;

namespace LeafBidAPI.Helpers;

public static class AuctionHelper
{
    public static double GetProductDurationSeconds(RegisteredProduct product)
    {
        const double minDurationSeconds = 30;
        const bool useMaxDuration = false;
        const double maxDurationSeconds = 300;

        decimal startPrice = product.MaxPrice ?? product.MinPrice;
        decimal minPrice = product.MinPrice;
        decimal rangeCents = (startPrice - minPrice) * 100;

        decimal startCents = startPrice * 100;
        decimal baseDecreaseCentsPerSecond = startCents * 0.05m;
        
        if (baseDecreaseCentsPerSecond <= 0) return 0;

        double impliedSeconds = (double)(rangeCents / baseDecreaseCentsPerSecond);
        double durationSeconds = Math.Max(minDurationSeconds, impliedSeconds);

        if (useMaxDuration)
        {
            durationSeconds = Math.Min(durationSeconds, maxDurationSeconds);
        }

        return durationSeconds;
    }
}
