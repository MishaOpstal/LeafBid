namespace LeafBidAPI.DTOs.AuctionSaleProduct;

public class UpdateAuctionSaleProductDto
{
    /// <summary>
    /// Data required to update an auction sale product
    /// </summary>

    public required int Id { get; set; }

    public int AuctionSaleId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
}