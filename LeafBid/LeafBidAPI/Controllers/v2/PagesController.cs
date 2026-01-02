using LeafBidAPI.DTOs.Page;
using LeafBidAPI.Enums;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeafBidAPI.Controllers.v2;

/// <summary>
/// Endpoints for retrieving auction pages that combine auctions and their products.
/// </summary>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
// [Authorize]
[AllowAnonymous]
[Produces("application/json")]
public class PagesController(IPagesServices pagesServices) : ControllerBase
{
    /// <summary>
    /// Get the closest upcoming auction and its products for a given clock location.
    /// </summary>
    /// <param name="clockLocationEnum">The clock location for which to retrieve the closest auction.</param>
    /// <returns>
    /// A DTO containing the closest auction at the specified clock location
    /// and the products belonging to that auction.
    /// </returns>
    [HttpGet("closest/{clockLocationEnum}")]
    [ProducesResponseType(typeof(GetAuctionWithProductsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetAuctionWithProductsDto>> GetAuctionWithProducts(
        ClockLocationEnum clockLocationEnum)
    {
        try
        {
            GetAuctionWithProductsDto auction = await pagesServices.GetAuctionWithProducts(clockLocationEnum);
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
    [ProducesResponseType(typeof(GetAuctionWithProductsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAuctionWithProductsDto>> GetAuctionWithProductsById(int auctionId)
    {
        try
        {
            GetAuctionWithProductsDto result = await pagesServices.GetAuctionWithProductsById(auctionId);
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// returns the active auction per clock location
    /// </summary>
    /// <returns>A DTO containing the active auction and its products for the specified clock location.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetAuctionWithProductsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetAuctionPerActiveClockLocationDto>> GetActiveAuctionWithProducts()
    {
        try
        {
            GetAuctionPerActiveClockLocationDto result = await pagesServices.GetAuctionPerActiveClockLocation();
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}