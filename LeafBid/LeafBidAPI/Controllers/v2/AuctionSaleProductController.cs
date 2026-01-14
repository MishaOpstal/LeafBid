using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.DTOs.User;
using LeafBidAPI.Enums;
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
public class AuctionSaleProductController(
    IUserService userService,
    IAuctionSaleProductService auctionSaleProductService) : ControllerBase
{
    /// <summary>
    /// Get all auction sale products.
    /// </summary>
    /// <returns>A list of all auction sale products.</returns>
    [HttpGet]
    [Authorize(Policy = PolicyTypes.AuctionSales.View)]
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
    [Authorize(Policy = PolicyTypes.AuctionSales.View)]
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
    /// Get auction sale products history for a registered product.
    /// </summary>
    /// <param name="id">The registered product ID.</param>
    /// <returns>A list of recent auction sales for the specified registered product.</returns>
    [HttpGet("history/{id:int}")]
    [Authorize(Policy = PolicyTypes.Products.View)]
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
    /// <returns>A list of recent auction sales for the specified registered product, including company sales.</returns>
    [HttpGet("history/{id:int}/company")]
    [Authorize(Policy = PolicyTypes.Products.View)]
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
    /// <returns>A list of recent auction sales for the specified registered product, excluding company sales.</returns>
    [HttpGet("history/{id:int}/not-company")]
    [Authorize(Policy = PolicyTypes.Products.View)]
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
    [Authorize(Policy = PolicyTypes.Products.View)]
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
            List<AuctionSaleProductResponse> products =
                await auctionSaleProductService.GetAuctionSaleProductsByUserId(me.UserData.Id);
            return Ok(products);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get auction sale products for the logged-in user's company.
    /// </summary>
    /// <returns>A list of auction sale products for the logged-in user.</returns>
    [HttpGet("company")]
    [Authorize(Policy = PolicyTypes.Products.View)]
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

            List<AuctionSaleProductResponse> products =
                await auctionSaleProductService.GetAuctionSaleProductsByCompanyId(me.UserData.CompanyId.Value);
            return Ok(products);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get auction sale products chart data for the logged-in user's company
    /// </summary>
    /// <returns>A list of auction sale products for the logged-in user.</returns>
    [HttpGet("chart")]
    [Authorize(Policy = PolicyTypes.Products.View)]
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

        SaleChartResponse chartData =
            await auctionSaleProductService.GetSaleChartDataByCompany(me.UserData.CompanyId.Value);
        return Ok(chartData);
    }
}