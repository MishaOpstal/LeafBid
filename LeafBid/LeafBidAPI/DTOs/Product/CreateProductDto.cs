namespace LeafBidAPI.DTOs.Product;

public class CreateProductDto
{
    /// <summary>
    /// Data required to create a product
    /// </summary>

    public required string Name { get; set; }

    public required string Description { get; set; }
    public required string Picture { get; set; }
    public required string Species { get; set; }
}