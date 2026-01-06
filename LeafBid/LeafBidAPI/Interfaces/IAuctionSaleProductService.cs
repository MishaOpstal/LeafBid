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
    
    Task<List<AuctionSaleProductResponse>> GetAuctionSaleProductsByUserId(string userId);
}