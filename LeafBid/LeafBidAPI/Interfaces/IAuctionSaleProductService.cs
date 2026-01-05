using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;

namespace LeafBidAPI.Interfaces;

public interface IAuctionSaleProductService
{
    /// <summary>
    /// Get all auction sale products.
    /// </summary>
    /// <returns>A list of auction sale products.</returns>
    Task<List<AuctionSaleProduct>> GetAuctionSaleProducts();
    
    /// <summary>
    /// Get a single auction sale product by ID.
    /// </summary>
    /// <param name="id">The ID of the auction sale product.</param>
    /// <returns>The auction sale product.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no auction sale product is found with the specified ID.
    /// </exception>
    Task<AuctionSaleProduct> GetAuctionSaleProductById(int id);

    /// <summary>
    /// Create a new auction sale product.
    /// </summary>
    /// <param name="auctionSaleProductData">The data used to create the auction sale product.</param>
    /// <returns>The created auction sale product.</returns>
    Task<AuctionSaleProduct> CreateAuctionSaleProduct(CreateAuctionSaleProductDto auctionSaleProductData);

    /// <summary>
    /// Update an existing auction sale product.
    /// </summary>
    /// <param name="id">The ID of the auction sale product.</param>
    /// <param name="auctionSaleProductData">The updated auction sale product data.</param>
    /// <returns>The updated auction sale product.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no auction sale product is found with the specified ID.
    /// </exception>
    Task<AuctionSaleProduct> UpdateAuctionSaleProduct(int id, UpdateAuctionSaleProductDto auctionSaleProductData);
    
    /// <summary>
    /// Get auction sale products history for a registered product excluding company products.
    /// </summary>
    /// <param name="registeredProductId">The registered product ID.</param>
    /// <returns>A list of recent auction sales for the specified registered product excluding company products.</returns>
    Task<List<AuctionSaleProductResponse>> GetAuctionSaleProductsHistoryNotCompany(int registeredProductId);
    
    /// <summary>
    /// Get auction sale products history for a registered product from the same company.
    /// </summary>
    /// <param name="registeredProductId">The registered product ID.</param>
    /// <returns>A list of recent auction sales for the specified registered product from the same company.</returns>
    Task<List<AuctionSaleProductResponse>> GetAuctionSaleProductsHistoryCompany(int registeredProductId);
    
    /// <summary>
    /// Get auction sale products by user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of auction sale products associated with the specified user ID.</returns>
    
    Task<List<AuctionSaleProductResponse>> GetAuctionSaleProductsByUserId(string userId);
}