namespace LeafBidAPI.DTOs.Product;

public class UpdateProductDto
{
    /// <summary>
    /// Data required to update a product
    /// </summary>

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Picture { get; set; }
    public string? Species { get; set; }
}