namespace LeafBidAPI.Configuration;

public sealed class AuctionTimerSettings
{
    public bool UseMaxDurationForAuctionTimer { get; init; }
    public int MinDurationForAuctionTimer { get; init; }
    public int MaxDurationForAuctionTimer { get; init; }
}
