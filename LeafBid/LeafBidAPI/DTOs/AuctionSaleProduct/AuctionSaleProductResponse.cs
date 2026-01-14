namespace LeafBidAPI.DTOs.AuctionSaleProduct;

public class AuctionSaleProductResponse
{
    public required int Quantity { get; set; }
    public required decimal Price { get; set; }
    public required DateTime Date { get; set; }
    public required Models.RegisteredProduct RegisteredProduct { get; set; }
    public required Models.Product Product { get; set; }
    public required Models.Company Company { get; set; }
}
