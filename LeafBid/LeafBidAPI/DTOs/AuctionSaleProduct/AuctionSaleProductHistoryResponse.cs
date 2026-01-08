namespace LeafBidAPI.DTOs.AuctionSaleProduct;

public class AuctionSaleProductHistoryResponse
{
    public required decimal AvgPrice { get; set; }
    public required List<AuctionSaleProductHistorySalesDto> RecentSales { get; set; }
}