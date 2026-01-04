namespace LeafBidAPI.DTOs.RegisteredProduct;

public class CreateRegisteredProductEndpointDto
{
    /// <summary>
    /// Data required to register a product
    /// </summary>
    public required string ProductId { get; set; }
    public required decimal MinPrice { get; set; }
    public required int Stock { get; set; }
    public required string Region { get; set; }
    public required DateTime HarvestedAt { get; set; }
    public double? PotSize { get; set; }
    public double? StemLength { get; set; }
}