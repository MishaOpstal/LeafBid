using System.Text.Json.Serialization;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace LeafBidAPI.Models;

/// <summary>
/// Represents the association between auction sales and products.
/// </summary>
public class AuctionSaleProduct
{
    /// <summary>
    /// Unique identifier for the auction sales product entry.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Identifier of the auction sale associated with the auction sale product
    /// </summary>
    public required int AuctionSaleId { get; set; }
    
    [JsonIgnore]
    public AuctionSale? AuctionSale { get; set; }
    
    /// <summary>
    /// Identifier of the registered product associated with the auction sale product
    /// </summary>
    public required int RegisteredProductId { get; set; }
    
    [JsonIgnore]
    public Product? RegisteredProduct { get; set; }
    
    /// <summary>
    /// Quantity of the product in the auction sale.
    /// </summary>
    public required int Quantity { get; set; }
    
    /// <summary>
    /// Price of the product in the auction sale.
    /// </summary>
    [Decimal(10,2)]
    public required decimal Price { get; set; }
}