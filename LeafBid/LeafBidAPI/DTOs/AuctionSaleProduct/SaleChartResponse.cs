namespace LeafBidAPI.DTOs.AuctionSaleProduct;

public class SaleChartResponse
{
    public required List<SaleChartDataPoint> CurrentMonthData { get; set; }
    public required List<SaleChartDataPoint> AllTimeData { get; set; }
}

public class SaleChartDataPoint
{
    public required string ProductName { get; set; }
    public required decimal Price { get; set; }
}