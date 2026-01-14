using LeafBidAPI.DTOs.Auction;
using LeafBidAPI.DTOs.Product;
using LeafBidAPI.DTOs.RegisteredProduct;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using LeafBidAPI.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeafBidAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
[Produces("application/json")]
public class AuctionController(IAuctionService auctionService, IProductService productService) : ControllerBase
{
    /// <summary>
    /// Get all auctions.
    /// </summary>
    /// <returns>A list of all auctions.</returns>
    [HttpGet]
    [Authorize(Policy = PolicyTypes.Auctions.View)]
    [ProducesResponseType(typeof(List<Auction>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Auction>>> GetAuctions()
    {
        List<Auction> auctions = await auctionService.GetAuctions(User);
        return Ok(auctions);
    }

    /// <summary>
    /// Get an auction by ID.
    /// </summary>
    /// <param name="id">The auction ID.</param>
    /// <returns>The requested auction.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = PolicyTypes.Auctions.View)]
    [ProducesResponseType(typeof(Auction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Auction>> GetAuction(int id)
    {
        try
        {
            Auction auction = await auctionService.GetAuctionById(id, User);
            return Ok(auction);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Create a new auction.
    /// </summary>
    /// <param name="auctionData">The auction creation data.</param>
    /// <returns>The created auction.</returns>
    [HttpPost]
    [Authorize(Policy = PolicyTypes.Auctions.Manage)]
    [ProducesResponseType(typeof(Auction), StatusCodes.Status201Created)]
    public async Task<ActionResult<Auction>> CreateAuction([FromBody] CreateAuctionDto auctionData)
    {
        Auction created = await auctionService.CreateAuction(auctionData, User);

        return CreatedAtAction(
            actionName: nameof(GetAuction),
            routeValues: new { id = created.Id, version = "2.0" },
            value: created
        );
    }
    
    /// <summary>
    /// Forcibly start an auction by setting the StartDate to 10 seconds in the future
    /// </summary>
    /// <param name="id">The auction ID</param>
    /// <returns>The started auction</returns>
    [HttpPut("start/{id:int}")]
    [Authorize(Policy = PolicyTypes.Auctions.Manage)]
    [ProducesResponseType(typeof(Auction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Auction>> StartAuction(int id)
    {
        try
        {
            Auction started = await auctionService.StartAuction(id);
            return Ok(started);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    /// <summary>
    /// Forcibly stop and (soft)delete the given auction
    /// </summary>
    /// <param name="id">The auction ID</param>
    /// <returns>The stopped auction</returns>.
    [HttpDelete("stop/{id:int}")]
    [Authorize(Policy = PolicyTypes.Auctions.Manage)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> StopAuction(int id)
    {
        try
        {
            bool started = await auctionService.StopAuction(id);
            return Ok(started);
        }
        catch (AuctionAlreadyFinishedException)
        {
            return BadRequest("Auction is already finished.");
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Update an existing auction.
    /// </summary>
    /// <param name="id">The auction ID.</param>
    /// <param name="updatedAuction">The updated auction data.</param>
    /// <returns>The updated auction.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyTypes.Auctions.Manage)]
    [ProducesResponseType(typeof(Auction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Auction>> UpdateAuction(
        int id,
        [FromBody] UpdateAuctionDto updatedAuction)
    {
        try
        {
            Auction updated = await auctionService.UpdateAuction(id, updatedAuction);
            return Ok(updated);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get registered products by auction ID.
    /// </summary>
    /// <param name="auctionId">The auction ID.</param>
    /// <returns>A list of registered products belonging to the auction.</returns>
    [HttpGet("products/{auctionId:int}")]
    [Authorize(Policy = PolicyTypes.Auctions.View)]
    [Authorize(Policy = PolicyTypes.Products.View)]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ProductResponse>>> GetRegisteredProductsByAuctionId(int auctionId)
    {
        try
        {
            List<RegisteredProduct> registeredProducts =
                await auctionService.GetRegisteredProductsByAuctionId(auctionId);

            List<RegisteredProductResponse> registeredProductResponses = registeredProducts
                .Select(productService.CreateRegisteredProductResponse)
                .ToList();

            return Ok(registeredProductResponses);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}