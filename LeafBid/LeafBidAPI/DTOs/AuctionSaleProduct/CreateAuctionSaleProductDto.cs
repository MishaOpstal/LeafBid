namespace LeafBidAPI.DTOs.AuctionSaleProduct;

public class CreateAuctionSaleProductDto
{
    /// <summary>
    /// Data required to create an auction sale product
    /// </summary>

    public required int AuctionSaleId { get; set; }

    public required int RegisteredProductId { get; set; }
    public required int Quantity { get; set; }
    public required decimal PricePerUnit { get; set; }
}