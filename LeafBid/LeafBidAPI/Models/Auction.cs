using System.Text.Json.Serialization;
using LeafBidAPI.Enums;

namespace LeafBidAPI.Models;

/// <summary>
/// Represents an auction
/// </summary>

public class Auction
{
    /// <summary>
    /// Unique identifier for the auction
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Start date of the auction
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// The start time of the next product in the auction.
    /// This is used to manage the delay between products.
    /// </summary>
    public DateTime? NextProductStartTime { get; set; }
    
    public ClockLocationEnum ClockLocationEnum { get; set; }
    
    /// <summary>
    /// Is the auction live?
    /// </summary>
    public bool IsLive { get; set; }

    /// <summary>
    /// Is the auction visible?
    /// </summary>
    public bool IsVisible { get; set; }
    
    /// <summary>
    /// Identifier of the user (auctioneer) associated with the auction
    /// </summary>
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User? User { get; set; }
}