using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.DTOs.User;
using LeafBidAPI.Enums;
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
    public async Task<ActionResult<AuctionSaleProductResponse>> GetAuctionSaleProductsByUserId()
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
    /// Get auction sale products for the logged-in user.
    /// </summary>
    /// <returns>A list of auction sale products for the logged-in user.</returns>
    [HttpGet("company")]
    [Authorize(Roles = "Provider")]
    [ProducesResponseType(typeof(AuctionSaleProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionSaleProductResponse>> GetAuctionSaleProductsByCompanyId()
    {
        try
        {
            LoggedInUserResponse me = await userService.GetLoggedInUser(User);
            if (me.UserData == null)
            {
                return Unauthorized("User data not found");
            }
            //grab auction sale product for the user
            if (me.UserData.CompanyId == null)
            {
                return BadRequest("User does not belong to a company");
            }
            List<AuctionSaleProductResponse> products = await auctionSaleProductService.GetAuctionSaleProductsByCompanyId(me.UserData.CompanyId.Value);
            return Ok(products);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    [HttpGet("chart")]
    [Authorize(Roles = "Provider")]
    [ProducesResponseType(typeof(AuctionSaleProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleChartResponse>> GetSaleChartData()
    {
        LoggedInUserResponse me = await userService.GetLoggedInUser(User);
        if (me.UserData == null)
        {
            return Unauthorized("User data not found");
        }

        if (me.UserData.CompanyId == null)
        {
            return BadRequest("User does not belong to a company");
        }
        
        SaleChartResponse chartData = await auctionSaleProductService.GetSaleChartDataByCompany(me.UserData.CompanyId.Value);
        return Ok(chartData);
    }
}