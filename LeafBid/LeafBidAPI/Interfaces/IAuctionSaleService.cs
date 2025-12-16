using LeafBidAPI.DTOs.AuctionSale;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;

namespace LeafBidAPI.Interfaces;

public interface IAuctionSaleService
{
    /// <summary>
    /// Get all auction sales.
    /// </summary>
    /// <returns>A list of all auction sales.</returns>
    Task<List<AuctionSale>> GetAuctionSales();

    /// <summary>
    /// Get a single auction sale by ID.
    /// </summary>
    /// <param name="id">The ID of the auction sale.</param>
    /// <returns>The auction sale with the specified ID.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no auction sale is found with the specified ID.
    /// </exception>
    Task<AuctionSale> GetAuctionSaleById(int id);

    /// <summary>
    /// Create a new auction sale.
    /// </summary>
    /// <param name="auctionSaleData">The data used to create the auction sale.</param>
    /// <returns>The created auction sale.</returns>
    Task<AuctionSale> CreateAuctionSale(CreateAuctionSaleDto auctionSaleData);
}