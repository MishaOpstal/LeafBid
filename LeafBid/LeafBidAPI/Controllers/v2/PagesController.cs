using LeafBidAPI.DTOs.Page;
using LeafBidAPI.Enums;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeafBidAPI.Controllers.v2;

/// <summary>
/// Endpoints for retrieving auction pages that combine auctions and their products.
/// </summary>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
[Produces("application/json")]
public class PagesController(IPagesServices pagesServices) : ControllerBase
{
    /// <summary>
    /// Get all auctions between today and the next 24 hours (1 day) and its products for a given clock location.
    /// </summary>
    /// <param name="clockLocationEnum">The clock location for which to retrieve the closest auction.</param>
    /// <returns>
    /// The closest auction at the specified clock location
    /// and the products belonging to that auction.
    /// </returns>
    [HttpGet("closest/{clockLocationEnum}")]
    [Authorize(Policy = PolicyTypes.Auctions.View)]
    [Authorize(Policy = PolicyTypes.Products.View)]
    [ProducesResponseType(typeof(GetAuctionWithProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetAuctionWithProductsResponse>> GetClosestAuctionsWithProducts(
        ClockLocationEnum clockLocationEnum)
    {
        try
        {
            List<GetAuctionWithProductsResponse> auction = await pagesServices.GetClosestAuctionsWithProducts(clockLocationEnum);
            return Ok(auction);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get a specific auction and its products by auction ID.
    /// </summary>
    /// <param name="auctionId">The ID of the auction.</param>
    /// <returns>
    /// A DTO containing the auction with the given ID and the products
    /// associated with that auction.
    /// </returns>
    [HttpGet("{auctionId:int}")]
    [Authorize(Policy = PolicyTypes.Auctions.View)]
    [Authorize(Policy = PolicyTypes.Products.View)]
    [ProducesResponseType(typeof(GetAuctionWithProductsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAuctionWithProductsResponse>> GetAuctionWithProductsById(int auctionId)
    {
        try
        {
            GetAuctionWithProductsResponse result = await pagesServices.GetAuctionWithProductsById(auctionId);
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// returns all the auctions per clock location
    /// </summary>
    /// <returns>A DTO containing the active auction and its products for the specified clock location.</returns>
    [HttpGet]
    [Authorize(Policy = PolicyTypes.Auctions.View)]
    [Authorize(Policy = PolicyTypes.Products.View)]
    [ProducesResponseType(typeof(GetAuctionWithProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetAuctionWithProductsResponse>> GetAuctionsWithProductsPerClockLocation()
    {
        try
        {
            List<GetAuctionWithProductsResponse> result = await pagesServices.GetAuctionsWithProductsPerClockLocation();
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}