namespace LeafBidAPI.DTOs.AuctionSaleProduct;

public class AuctionEventResponse
{
    public required Models.RegisteredProduct RegisteredProduct { get; set; }
    public DateTime? NextProductStartTime { get; set; }
    public bool IsSuccess { get; set; }
}
