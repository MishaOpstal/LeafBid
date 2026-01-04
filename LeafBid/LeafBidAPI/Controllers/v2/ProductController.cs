using LeafBidAPI.DTOs.Product;
using LeafBidAPI.DTOs.RegisteredProduct;
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
public class ProductController(IProductService productService) : ControllerBase
{
    /// <summary>
    /// Get all products.
    /// </summary>
    /// <returns>A list of all products.</returns>
    [HttpGet]
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
    /// Get a product by ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The requested product.</returns>
    [HttpGet("{id:int}")]
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
    [Authorize(Roles = "Auctioneer")]
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
    /// <param name="productId"></param>
    /// <param name="userId"></param>
    /// <returns>The created product.</returns>
    /// <exception cref="Exception">Thrown when the main product cannot be found.</exception>
    [HttpPost("/registeredCreate/{ProductId:int}")]
    [ProducesResponseType(typeof(RegisteredProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegisteredProductResponse>> CreateRegisteredProduct(
        [FromBody] CreateRegisteredProductEndpointDto productData, int productId, string userId)
    {
        try
        {
            RegisteredProduct registeredProduct = await productService.CreateProductDeliveryGuy(productData, productId, userId);
            RegisteredProductResponse registeredProductResponse = productService.CreateRegisteredProductResponse(registeredProduct);
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
            Console.WriteLine(e);
            throw;
        }

    }

    /// <summary>
    /// Update an existing product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="updatedProduct">The updated product data.</param>
    /// <returns>The updated product.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Provider")]
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
    /// Delete a product by ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>No content if deletion succeeded.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Provider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        bool deleted = await productService.DeleteProduct(id);
        return deleted ? NoContent() : Problem("Product deletion failed.");
    }
}