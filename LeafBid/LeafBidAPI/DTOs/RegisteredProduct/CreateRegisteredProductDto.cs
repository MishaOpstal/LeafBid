namespace LeafBidAPI.DTOs.RegisteredProduct;

public class CreateRegisteredProductDto
{
    /// <summary>
    /// Data required to register a product
    /// </summary>
    public required string UserId { get; set; }
    public required decimal MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public required int Stock { get; set; }
    public required string Region { get; set; }
    public required DateTime HarvestedAt { get; set; }
    public double? PotSize { get; set; }
    public double? StemLength { get; set; }
}