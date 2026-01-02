using System.Text.Json.Serialization;
using LeafBidAPI.Enums;

namespace LeafBidAPI.Models;

/// <summary>
/// Represents the products associated with an auction
/// </summary>
public class AuctionProduct
{
    /// <summary>
    /// Associated auction id.
    /// </summary>
    public required int AuctionId { get; set; }

    [JsonIgnore] public Auction? Auction { get; set; }

    /// <summary>
    /// Associated registered product id.
    /// </summary>
    public required int RegisteredProductId { get; set; }

    [JsonIgnore]
    public RegisteredProduct? RegisteredProduct { get; set; }

    /// <summary>
    /// Order in auction. (A lower number will be served first)
    /// </summary>
    public required int ServeOrder { get; set; }

    /// <summary>
    /// Products left
    /// </summary>
    public required int AuctionStock { get; set; }
}