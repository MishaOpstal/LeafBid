using LeafBidAPI.Enums;

namespace LeafBidAPI.DTOs.Auction;

public class UpdateAuctionDto
{
    public required DateTime StartTime { get; set; }
    public required ClockLocationEnum ClockLocationEnum { get; set; }
}