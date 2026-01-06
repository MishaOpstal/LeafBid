using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;
using LeafBidAPI.DTOs.Product;

namespace LeafBidAPI.DTOs.RegisteredProduct;

public class RegisteredProductResponse
{
    public required int Id { get; set; }
    public required  ProductResponse Product { get; set; }
    [Decimal(10, 2)] public required decimal MinPrice { get; set; }
    [Decimal(10, 2)] public decimal? MaxPrice { get; set; } 
    public required int Stock { get; set; }
    public required string Region { get; set; }
    public required DateTime HarvestedAt { get; set; }
    public double? PotSize { get; set; }
    public double? StemLength { get; set; }
    public required string ProviderUserName { get; set; }
}