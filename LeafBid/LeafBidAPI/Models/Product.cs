namespace LeafBidAPI.Models;

/// <summary>
/// Represents a product in the system.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the product.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the product.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Description of the product.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Picture URL of the product.
    /// </summary>
    public string? Picture { get; set; }

    /// <summary>
    /// Species of the product.
    /// </summary>
    public required string Species { get; set; }
}