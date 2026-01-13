using LeafBidAPI.Enums;

namespace LeafBidAPI.DTOs.Auction;

public class RegisteredProductForAuctionRequest
{
    /// <summary>
    /// Data required for binding a registered to an auction
    /// </summary>

    public required int Id { get; set; }
    public required decimal MaxPrice { get; set; }
}