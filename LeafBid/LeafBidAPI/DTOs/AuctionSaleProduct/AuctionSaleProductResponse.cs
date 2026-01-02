namespace LeafBidAPI.DTOs.AuctionSaleProduct;

public class AuctionSaleProductResponse
{
    public required string Name { get; set; }
    
    public required string Picture { get; set; }
    public required int Quantity { get; set; }
    public required decimal Price { get; set; }
    public required DateTime Date { get; set; }
}