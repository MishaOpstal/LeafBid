using System.Security.Claims;
using LeafBidAPI.DTOs.Auction;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;

namespace LeafBidAPI.Interfaces;

public interface IAuctionService
{
    /// <summary>
    /// Get all auctions.
    /// </summary>
    /// <returns>A list of all auctions.</returns>
    Task<List<Auction>> GetAuctions();

    /// <summary>
    /// Get a single auction by its ID.
    /// </summary>
    /// <param name="id">The auction ID.</param>
    /// <returns>The auction with the specified ID.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no auction is found with the specified ID.
    /// </exception>
    Task<Auction> GetAuctionById(int id);

    /// <summary>
    /// Create a new auction for the current user.
    /// </summary>
    /// <param name="auctionData">The auction creation data, including products.</param>
    /// <param name="user">The current authenticated user principal.</param>
    /// <returns>The created auction.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when the current user cannot be resolved from the principal.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the current user does not have the <c>Auctioneer</c> role.
    /// </exception>
    /// <exception cref="ProductAlreadyAssignedException">
    /// Thrown when one or more products in the request are already assigned to an auction.
    /// </exception>
    Task<Auction> CreateAuction(CreateAuctionDto auctionData, ClaimsPrincipal user);
    
    /// <summary>
    /// Update an existing auction.
    /// </summary>
    /// <param name="id">The auction ID.</param>
    /// <param name="updatedAuction">The updated auction data.</param>
    /// <returns>The updated auction.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no auction is found with the specified ID.
    /// </exception>
    Task<Auction> UpdateAuction(int id, UpdateAuctionDto updatedAuction);

    /// <summary>
    /// Get all registered products for a given auction ID, ordered by serve order.
    /// </summary>
    /// <param name="auctionId">The auction ID.</param>
    /// <returns>A list of registered products belonging to the auction.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no registered products are found for the specified auction.
    /// </exception>
    Task<List<RegisteredProduct>> GetRegisteredProductsByAuctionId(int auctionId);
}