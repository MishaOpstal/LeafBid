using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.Enums;
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
    /// Get auction sale products history for a registered product.
    /// </summary>
    /// <param name="registeredProductId">The registered product ID.</param>
    /// <param name="scope">The scope of the history to retrieve (All, OnlyCompany, ExcludeCompany).</param>
    /// <param name="includeCompanyName">Whether to include the company name in the response.</param>
    /// <param name="limit">The maximum number of records to retrieve. Default is 10.</param>
    /// <returns>A list of recent auction sales for the specified registered product.</returns>
    Task<AuctionSaleProductHistoryResponse> GetAuctionSaleProductsHistory(
        int registeredProductId,
    HistoryEnum scope,
    bool includeCompanyName,
    int? limit = 10);
    
    /// <summary>
    /// Get auction sale products by user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of auction sale products associated with the specified user ID.</returns>
    Task<List<AuctionSaleProductResponse>> GetAuctionSaleProductsByUserId(string userId);
    
    /// <summary>
    /// Get auction sale products by company ID.
    /// </summary>
    /// <param name="companyId">The company ID.</param>
    /// <returns>A list of auction sale products associated with the specified company ID.</returns>
    Task<List<AuctionSaleProductResponse>> GetAuctionSaleProductsByCompanyId(int companyId);

    Task<SaleChartResponse> GetSaleChartDataByCompany(int companyId);

    /// <summary>
    /// Buys a product from an auction.
    /// </summary>
    /// <param name="buyData">The data for the purchase.</param>
    /// <param name="userId">The ID of the user making the purchase.</param>
    /// <returns>The updated RegisteredProduct and the new auction start date.</returns>
    Task<AuctionEventResponse> BuyProduct(BuyProductDto buyData, string userId);

    /// <summary>
    /// Expires a product from an auction (sets stock to 0).
    /// </summary>
    /// <param name="registeredProductId">The ID of the registered product to expire.</param>
    /// <param name="auctionId">The ID of the auction.</param>
    /// <returns>The updated RegisteredProduct and the new auction start date.</returns>
    Task<AuctionEventResponse> ExpireProduct(int registeredProductId, int auctionId);
}