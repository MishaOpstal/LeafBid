using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace LeafBidAPI.DTOs.Product;

public class ProductResponse
{
    public required int Id { get; set; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public string? Picture { get; set; }

    public required string Species { get; set; }
}