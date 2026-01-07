namespace LeafBidAPI.DTOs.AuctionSaleProduct;

public class BuyProductDto
{
    public required int RegisteredProductId { get; set; }
    public required int AuctionId { get; set; }
    public required int Quantity { get; set; }
    public required decimal PricePerUnit { get; set; }
}
