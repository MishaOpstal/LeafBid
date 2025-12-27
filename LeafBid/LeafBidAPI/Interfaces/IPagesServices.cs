using LeafBidAPI.DTOs.Page;
using LeafBidAPI.Enums;
using LeafBidAPI.Exceptions;

namespace LeafBidAPI.Interfaces;

public interface IPagesServices
{
    /// <summary>
    /// Get the closest upcoming auction and its products for a given clock location.
    /// </summary>
    /// <param name="clockLocation">The clock location for which to retrieve the auction and products.</param>
    /// <returns>
    /// A DTO containing the closest auction at the specified clock location
    /// and the corresponding list of products.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no auction is found for the specified clock location,
    /// or when no products are found for the located auction.
    /// </exception>
    Task<GetAuctionWithProductsDto> GetAuctionWithProducts(ClockLocationEnum clockLocation);

    /// <summary>
    /// Get a specific auction and its products by auction ID.
    /// </summary>
    /// <param name="auctionId">The ID of the auction.</param>
    /// <returns>
    /// A DTO containing the auction with the given ID
    /// and the corresponding list of products.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no auction is found with the specified ID,
    /// or when no products are found for the located auction.
    /// </exception>
    Task<GetAuctionWithProductsDto> GetAuctionWithProductsById(int auctionId);
    
}