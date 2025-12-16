using System.Text.Json.Serialization;

namespace LeafBidAPI.Models;

/// <summary>
/// Represents a sale made at an auction.
/// </summary>
public class AuctionSale
{
    /// <summary>
    /// Unique identifier for the auction sale.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Identifier for the auction where the sale took place.
    /// </summary>
    public required int AuctionId { get; set; }
    
    [JsonIgnore]
    public Auction? Auction { get; set; }
    
    /// <summary>
    /// Unique identifier for the user who made the purchase.
    /// </summary>
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User? User { get; set; }
    
    /// <summary>
    /// Date and time when the sale occurred.
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Payment reference associated with the sale.
    /// </summary>
    public required string PaymentReference { get; set; }
}