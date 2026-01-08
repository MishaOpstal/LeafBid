namespace LeafBidAPI.DTOs.AuctionSaleProduct;

public class AuctionSaleProductHistorySalesDto
{
    public required int Quantity { get; set; }
    public required decimal Price { get; set; }
    public required DateTime Date { get; set; }
    public string? CompanyName { get; set; }
}