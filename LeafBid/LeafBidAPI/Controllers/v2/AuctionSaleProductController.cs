using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.DTOs.User;
using LeafBidAPI.Enums;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Hubs;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace LeafBidAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
// [Authorize]
[AllowAnonymous]
[Produces("application/json")]
public class AuctionSaleProductController(
    IUserService userService,
    IAuctionSaleProductService auctionSaleProductService,
    IHubContext<AuctionHub> hubContext) : ControllerBase
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
    /// <summary>
    /// Get auction sale products history for a registered product.
    /// </summary>
    /// <param name="id">The registered product ID.</param>
    /// <returns>A list of recent auction sales for the specified registered product.</returns>
    [HttpGet("history/{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(AuctionSaleProduct), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuctionSaleProductsHistory(int id)
    {
        try
        {
            AuctionSaleProductHistoryResponse products =
                await auctionSaleProductService.GetAuctionSaleProductsHistory(id, HistoryEnum.All, true);
            return Ok(products);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
                
    }
        
    /// <summary>
    /// Get auction sale products history for a registered product including company sales.
    /// </summary>
    /// <param name="id">The registered product ID.</param>
    /// <returns>A list of recent auction sales for the specified registered product including company sales.</returns>
    [HttpGet("history/{id:int}/company")]
    [Authorize]
    [ProducesResponseType(typeof(AuctionSaleProduct), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuctionSaleProductsHistoryCompany(int id)
    {
        try
        {
            AuctionSaleProductHistoryResponse products =
                await auctionSaleProductService.GetAuctionSaleProductsHistory(id, HistoryEnum.OnlyCompany, false);
            return Ok(products);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
                
    }
    
    /// <summary>
    /// Get auction sale products history for a registered product excluding company sales.
    /// </summary>
    /// <param name="id">The registered product ID.</param>
    /// <returns>A list of recent auction sales for the specified registered product excluding company sales.</returns>
    [HttpGet("history/{id:int}/not-company")]
    [Authorize]
    [ProducesResponseType(typeof(AuctionSaleProduct), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuctionSaleProductsHistoryNotCompany(int id)
    {
        try
        {
            AuctionSaleProductHistoryResponse products =
                await auctionSaleProductService.GetAuctionSaleProductsHistory(id, HistoryEnum.ExcludeCompany, true);
            return Ok(products);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
                
    }
    
    /// <summary>
    /// Get auction sale products for the logged-in user.
    /// </summary>
    /// <returns>A list of auction sale products for the logged-in user.</returns>
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

    /// <summary>
    /// Buys a product from an auction.
    /// </summary>
    /// <param name="buyData">The purchase data.</param>
    /// <returns>The updated registered product.</returns>
    [HttpPost("buy")]
    [Authorize(Roles = "Buyer")]
    [ProducesResponseType(typeof(RegisteredProduct), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegisteredProduct>> BuyProduct([FromBody] BuyProductDto buyData)
    {
        try
        {
            LoggedInUserResponse me = await userService.GetLoggedInUser(User);
            if (me.UserData == null)
            {
                return Unauthorized("User data not found");
            }

            AuctionEventResponse result = await auctionSaleProductService.BuyProduct(buyData, me.UserData.Id);

            if (result.IsSuccess)
            {
                // Notify all clients in the auction group
                await hubContext.Clients.Group(buyData.AuctionId.ToString()).SendAsync("ProductBought", new
                {
                    registeredProductId = result.RegisteredProduct.Id,
                    stock = result.RegisteredProduct.Stock,
                    quantityBought = buyData.Quantity,
                    newStartDate = result.NewStartDate
                });
            }

            return Ok(result.RegisteredProduct);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Expires a product from an auction (sets stock to 0).
    /// </summary>
    /// <param name="registeredProductId">The registered product ID.</param>
    /// <param name="auctionId">The auction ID.</param>
    /// <returns>The updated registered product.</returns>
    [HttpPost("expire/{registeredProductId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ExpireProduct(int registeredProductId, [FromQuery] int auctionId)
    {
        try
        {
            AuctionEventResponse result = await auctionSaleProductService.ExpireProduct(registeredProductId, auctionId);

            if (result.IsSuccess)
            {
                // Notify all clients in the auction group
                await hubContext.Clients.Group(auctionId.ToString()).SendAsync("ProductExpired", new
                {
                    registeredProductId = result.RegisteredProduct.Id,
                    newStartDate = result.NewStartDate
                });
                return Ok(result.RegisteredProduct);
            }

            return BadRequest("Product not yet expired or already handled.");
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}