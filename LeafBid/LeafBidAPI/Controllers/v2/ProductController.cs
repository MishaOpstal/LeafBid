using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.DTOs.Product;
using LeafBidAPI.DTOs.RegisteredProduct;
using LeafBidAPI.DTOs.User;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Hubs;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using LeafBidAPI.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace LeafBidAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
[Produces("application/json")]
public class ProductController(
    IProductService productService,
    IUserService userService,
    IHubContext<AuctionHub> hubContext
) : ControllerBase
{
    /// <summary>
    /// Get all products.
    /// </summary>
    /// <returns>A list of all products.</returns>
    [HttpGet]
    [Authorize(Policy = PolicyTypes.Products.View)]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductResponse>>> GetProducts()
    {
        List<Product> products = await productService.GetProducts();
        List<ProductResponse> productResponses = products
            .Select(productService.CreateProductResponse)
            .ToList();
        return Ok(productResponses);
    }

    /// <summary>
    /// Get all available products.
    /// </summary>
    /// <returns>A list of available products.</returns>
    [HttpGet("available")]
    [Authorize(Policy = PolicyTypes.Products.View)]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductResponse>>> GetAvailableProducts()
    {
        List<Product> products = await productService.GetAvailableProducts();
        List<ProductResponse> productResponses = products
            .Select(productService.CreateProductResponse)
            .ToList();
        return Ok(productResponses);
    }

    /// <summary>
    /// Get all available registered products.
    /// </summary>
    /// <returns>A list of available registered products.</returns>
    [HttpGet("available/registered")]
    [Authorize(Policy = PolicyTypes.Products.View)]
    [ProducesResponseType(typeof(List<RegisteredProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RegisteredProductResponse>>> GetAvailableRegisteredProducts()
    {
        List<RegisteredProduct> registeredProducts = await productService.GetAvailableRegisteredProducts();
        List<RegisteredProductResponse> registeredProductResponses = registeredProducts
            .Select(productService.CreateRegisteredProductResponse)
            .ToList();
        return Ok(registeredProductResponses);
    }

    /// <summary>
    /// Get a product by ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The requested product.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = PolicyTypes.Products.View)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> GetProductById(int id)
    {
        Product product = await productService.GetProductById(id);
        ProductResponse productResponse = productService.CreateProductResponse(product);
        return Ok(productResponse);
    }

    /// <summary>
    /// Get a registered product by the product id and user id keys.
    /// </summary>
    /// <param name="id">The registered product ID.</param>
    /// <returns>The requested registered product.</returns>
    [HttpGet("/registered/{id:int}")]
    [Authorize(Policy = PolicyTypes.Products.View)]
    [ProducesResponseType(typeof(RegisteredProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RegisteredProductResponse>> GetRegisteredProductById(int id)
    {
        RegisteredProduct registeredProduct =
            await productService.GetRegisteredProductById(id);
        RegisteredProductResponse registeredProductResponse =
            productService.CreateRegisteredProductResponse(registeredProduct);
        return Ok(registeredProductResponse);
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="productData">The product data.</param>
    /// <returns>The created product.</returns>
    [HttpPost]
    [Authorize(Policy = PolicyTypes.Products.Manage)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductDto productData)
    {
        Product product = await productService.CreateProduct(productData);
        ProductResponse productResponse = productService.CreateProductResponse(product);

        return CreatedAtAction(
            actionName: nameof(GetProductById),
            routeValues: new
            {
                id = productResponse.Id,
                version = "2.0"
            },
            value: productResponse
        );
    }

    /// <summary>
    /// Create a new registered product.
    /// </summary>
    /// <param name="productData">The product data.</param>
    /// <param name="productId">The id of the Product</param>
    /// <returns>The created product.</returns>
    /// <exception cref="NotFoundException">Thrown when the main product cannot be found.</exception>
    [HttpPost("registeredCreate/{ProductId:int}")]
    [Authorize(Policy = PolicyTypes.Products.ManageRegistered)]
    [ProducesResponseType(typeof(RegisteredProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RegisteredProductResponse>> CreateRegisteredProduct(
        [FromBody] CreateRegisteredProductEndpointDto productData, [FromRoute] int productId)
    {
        try
        {
            LoggedInUserResponse me = await userService.GetLoggedInUser(User);

            RegisteredProduct registeredProduct = await productService.CreateRegisteredProduct(
                productData,
                productId,
                me.UserData!.Id,
                me.UserData.CompanyId!.Value
            );

            RegisteredProductResponse registeredProductResponse =
                productService.CreateRegisteredProductResponse(registeredProduct);
            return CreatedAtAction(
                actionName: nameof(GetProductById),
                routeValues: new
                {
                    id = registeredProductResponse.Id,
                    version = "2.0"
                },
                value: registeredProductResponse
            );
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedException e)
        {
            return Unauthorized(e.Message);
        }
        catch (BadRequestException e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Update an existing product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="updatedProduct">The updated product data.</param>
    /// <returns>The updated product.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyTypes.Products.Manage)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(
        [FromRoute] int id,
        [FromBody] UpdateProductDto updatedProduct)
    {
        try
        {
            Product product = await productService.UpdateProduct(id, updatedProduct);
            ProductResponse updated = productService.CreateProductResponse(product);
            return Ok(updated);
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
    [Authorize(Policy = PolicyTypes.Products.Buy)]
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

            AuctionEventResponse result = await productService.BuyProduct(buyData, me.UserData.Id);

            if (result.IsSuccess)
            {
                // Notify all clients in the auction group
                await hubContext.Clients.Group(buyData.AuctionId.ToString()).SendAsync("ProductBought", new
                {
                    registeredProductId = result.RegisteredProduct.Id,
                    stock = result.RegisteredProduct.Stock,
                    quantityBought = buyData.Quantity,
                    nextProductStartTime = result.NextProductStartTime
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
    /// Delete a product by ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>No content if deletion succeeded.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyTypes.Products.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        bool deleted = await productService.DeleteProduct(id);
        return deleted ? NoContent() : Problem("Product deletion failed.");
    }
}