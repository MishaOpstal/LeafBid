using LeafBidAPI.DTOs.Product;
using LeafBidAPI.DTOs.RegisteredProduct;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;

namespace LeafBidAPI.Interfaces;

public interface IProductService
{
    /// <summary>
    /// Get all products.
    /// </summary>
    /// <returns>A list of all products.</returns>
    Task<List<Product>> GetProducts();

    /// <summary>
    /// Get all products that are not currently assigned to an auction.
    /// </summary>
    /// <returns>A list of available products.</returns>
    Task<List<Product>> GetAvailableProducts();
    
    /// <summary>
    /// Get all registered products that are not currently assigned to an auction.
    /// </summary>
    /// <returns>A list of available registered products.</returns>
    Task<List<RegisteredProduct>> GetAvailableRegisteredProducts();

    /// <summary>
    /// Get a product by ID, including its provider user.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product with the specified ID.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no product is found with the specified ID.
    /// </exception>
    Task<Product> GetProductById(int id);

    /// <summary>
    /// Get a registered product by its ID.
    /// </summary>
    /// <param name="id">The registered product ID.</param>
    /// <returns>The registered product with the specified product and user Ids.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no registered product is found with the specified IDs.
    /// </exception>
    Task<RegisteredProduct> GetRegisteredProductById(int id);

    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="productData">The product creation data, including an optional base64 image.</param>
    /// <returns>The created product.</returns>
    /// <exception cref="Exception">
    /// Thrown when the provided image cannot be processed or saved.
    /// </exception>
    Task<Product> CreateProduct(CreateProductDto productData);

    /// <summary>
    /// Create a new registered product, for the supplier.
    /// </summary>
    /// <param name="registeredProductData"></param>
    /// <param name="productId"></param>
    /// <param name="userId"></param>
    /// <param name="companyId"></param>
    /// <returns>The created product for the supplier.</returns>
    /// <exception cref="Exception">
    /// Thrown when the product cannot be created
    /// </exception>
    Task<RegisteredProduct> CreateRegisteredProduct(CreateRegisteredProductEndpointDto registeredProductData,
        int productId, string userId, int companyId);

    /// <summary>
    /// Update an existing product by ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="updatedProduct">The updated product data.</param>
    /// <returns>The updated product.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no product is found with the specified ID.
    /// </exception>
    Task<Product> UpdateProduct(int id, UpdateProductDto updatedProduct);

    /// <summary>
    /// Delete a product by ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>
    /// <c>true</c> if the product was found and deleted; otherwise <c>false</c>.
    /// </returns>
    Task<bool> DeleteProduct(int id);

    /// <summary>
    /// Create a DTO representation of a product.
    /// </summary>
    /// <param name="product">The product to map.</param>
    /// <returns>A <see cref="ProductResponse"/> populated from the product.</returns>
    ProductResponse CreateProductResponse(Product product);

    /// <summary>
    /// Create a DTO representation of a registered product.
    /// </summary>
    /// <param name="registeredProduct"></param>
    /// <returns>A <see cref="RegisteredProductResponse"/> populated from the registered product.</returns>
    RegisteredProductResponse CreateRegisteredProductResponse(RegisteredProduct registeredProduct);
}