using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.DTOs.User;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeafBidAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
// [Authorize]
[AllowAnonymous]
[Produces("application/json")]
public class AuctionSaleProductController(IUserService userService, IAuctionSaleProductService auctionSaleProductService) : ControllerBase
{
    /// <summary>
    /// Get all auction sale products.
    /// </summary>
    /// <returns>A list of all auction sale products.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<AuctionSaleProduct>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuctionSaleProduct>>> GetAuctionSaleProducts()
    {
        List<AuctionSaleProduct> items = await auctionSaleProductService.GetAuctionSaleProducts();
        return Ok(items);
    }

    /// <summary>
    /// Get an auction sale product by ID.
    /// </summary>
    /// <param name="id">The auction sale product ID.</param>
    /// <returns>The requested auction sale product.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AuctionSaleProduct), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionSaleProduct>> GetAuctionSaleProduct(int id)
    {
        try
        {
            AuctionSaleProduct item = await auctionSaleProductService.GetAuctionSaleProductById(id);
            return Ok(item);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Create a new auction sale product.
    /// </summary>
    /// <param name="auctionSaleProductData">The auction sale product data.</param>
    /// <returns>The created auction sale product.</returns>
    [HttpPost]
    [Authorize(Roles = "Provider")]
    [ProducesResponseType(typeof(AuctionSaleProduct), StatusCodes.Status201Created)]
    public async Task<ActionResult<AuctionSaleProduct>> CreateAuctionSaleProducts(
        [FromBody] CreateAuctionSaleProductDto auctionSaleProductData)
    {
        AuctionSaleProduct created = await auctionSaleProductService.CreateAuctionSaleProduct(auctionSaleProductData);

        return CreatedAtAction(
            actionName: nameof(GetAuctionSaleProduct),
            routeValues: new { id = created.Id, version = "2.0" },
            value: created
        );
    }

    /// <summary>
    /// Update an existing auction sale product.
    /// </summary>
    /// <param name="id">The auction sale product ID.</param>
    /// <param name="updatedAuctionSaleProduct">The updated auction sale product data.</param>
    /// <returns>The updated auction sale product.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Provider")]
    [ProducesResponseType(typeof(AuctionSaleProduct), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuctionSaleProduct>> UpdateAuctionSaleProducts(
        int id,
        [FromBody] UpdateAuctionSaleProductDto updatedAuctionSaleProduct)
    {
        AuctionSaleProduct updated =
            await auctionSaleProductService.UpdateAuctionSaleProduct(id, updatedAuctionSaleProduct);

        return Ok(updated);
    }
    
    [HttpGet("me")]
    [Authorize(Roles = "Buyer")]
    [ProducesResponseType(typeof(AuctionSaleProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionSaleProductResponse>> LoggedInUser()
    {
        try
        {
            LoggedInUserResponse me = await userService.GetLoggedInUser(User);
            if (me.UserData == null)
            {
                return Unauthorized("User data not found");
            }
            //grab auction sale product for the user
            List<AuctionSaleProductResponse> products = await auctionSaleProductService.GetAuctionSaleProductsByUserId(me.UserData.Id);
            return Ok(products);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}