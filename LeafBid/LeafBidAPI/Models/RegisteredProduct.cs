using System.Text.Json.Serialization;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace LeafBidAPI.Models;

public class RegisteredProduct
{
    /// <summary>
    /// Unique id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// represents a product in our system
    /// </summary>
    public required int ProductId { get; set; }
    
    [JsonIgnore]
    public Product? Product { get; set; }

    /// <summary>
    /// User id associated with the product.
    /// </summary>
     public required string UserId { get; set; }
    
    [JsonIgnore]
     public User? User { get; set; }

    /// <summary>
    /// Minimum Price of the product per unit.
    /// </summary>
    [Decimal(10,2)]
    public required decimal MinPrice { get; set; }
    
    /// <summary>
    /// Max Price of the product per unit.
    /// </summary>
    [Decimal(10,2)]
    public decimal? MaxPrice { get; set; }
    
    /// <summary>
    /// Stock quantity of the product.
    /// </summary>
    public required int Stock { get; set; }

    /// <summary>
    /// Region of the product.
    /// </summary>
    public required string Region { get; set; }

    /// <summary>
    /// Harvested date and time of the product.
    /// </summary>
    public required DateTime HarvestedAt { get; set; }
    
    /// <summary>
    /// Pot size of the product.
    /// </summary>
    public double? PotSize { get; set; }

    /// <summary>
    /// Stem length of the product.
    /// </summary>
    public double? StemLength { get; set; }
}