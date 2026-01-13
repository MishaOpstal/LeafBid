using LeafBidAPI.Enums;

namespace LeafBidAPI.DTOs.Auction;

public class CreateAuctionDto
{
    /// <summary>
    /// Data required to create a new auction
    /// </summary>

    public required DateTime StartDate { get; set; }

    public required ClockLocationEnum ClockLocationEnum { get; set; }
    public required RegisteredProductForAuctionRequest[] RegisteredProductForAuction { get; set; }
}