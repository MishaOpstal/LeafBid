using LeafBidAPI.Data;
using LeafBidAPI.DTOs.Product;
using LeafBidAPI.DTOs.RegisteredProduct;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace LeafBidAPI.Services;

public class ProductService(ApplicationDbContext context) : IProductService
{
    public async Task<List<Product>> GetProducts()
    {
        return await context.Products.ToListAsync();
    }
    
    public async Task<List<Product>> GetAvailableProducts()
    {
        List<Product> products = await context.Products
            .Where(p => !context.AuctionProducts.Any(ap => ap.RegisteredProduct != null && ap.RegisteredProduct.ProductId == p.Id))
            .ToListAsync();

        return products;
    }
    
    public async Task<Product> GetProductById(int id)
    {
        Product? product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        return product ?? throw new NotFoundException("Product not found");
    }
    
    public async Task<RegisteredProduct> GetRegisteredProductById(int id)
    {
        RegisteredProduct? registeredProduct = await context.RegisteredProducts
            .FirstOrDefaultAsync(
                p => p.Id == id
            );

        return registeredProduct ?? throw new NotFoundException("Registered product not found");
    }
    
    public async Task<Product> CreateProduct(CreateProductDto productData)
    {
        if (!string.IsNullOrEmpty(productData.Picture) && productData.Picture.StartsWith("data:image"))
        {
            try
            {
                string uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsDir);

                string base64Data = productData.Picture.Contains(',')
                    ? productData.Picture[(productData.Picture.IndexOf(',') + 1)..]
                    : productData.Picture;

                byte[] bytes = Convert.FromBase64String(base64Data);
                string fileName = $"{Guid.NewGuid()}.png";
                string filePath = Path.Combine(uploadsDir, fileName);

                using (Image image = Image.Load(bytes))
                {
                    const int targetSize = 800;

                    double scale = Math.Max(
                        (double)targetSize / image.Width,
                        (double)targetSize / image.Height
                    );

                    int resizedWidth = (int)(image.Width * scale);
                    int resizedHeight = (int)(image.Height * scale);

                    image.Mutate(x => x.Resize(resizedWidth, resizedHeight));

                    int cropX = (resizedWidth - targetSize) / 2;
                    int cropY = (resizedHeight - targetSize) / 2;

                    Rectangle cropRect = new(cropX, cropY, targetSize, targetSize);

                    image.Mutate(x => x.Crop(cropRect));

                    await image.SaveAsPngAsync(filePath);
                }

                productData.Picture = $"/uploads/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to process image", ex);
            }
        }

        Product product = new()
        {
            Name = productData.Name,
            Description = productData.Description,
            Picture = productData.Picture,
            Species = productData.Species,
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product;
    }

    public async Task<RegisteredProduct> AddProduct(int productId, CreateRegisteredProductDto registeredProductData)
    {
        RegisteredProduct registeredProduct = new()
        {
            ProductId = productId,
            UserId = registeredProductData.UserId,
            MinPrice = registeredProductData.MinPrice,
            MaxPrice = registeredProductData.MaxPrice,
            Stock = registeredProductData.Stock,
            Region = registeredProductData.Region,
            HarvestedAt = registeredProductData.HarvestedAt,
            PotSize = registeredProductData.PotSize,
            StemLength = registeredProductData.StemLength
        };

        context.RegisteredProducts.Add(registeredProduct);
        await context.SaveChangesAsync();

        return registeredProduct;
    }

    public async Task<Product> UpdateProduct(int id, UpdateProductDto updatedProduct)
    {
        Product? product = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }

        product.Name = updatedProduct.Name ?? product.Name;
        product.Description = updatedProduct.Description ?? product.Description;
        product.Picture = updatedProduct.Picture ?? product.Picture;
        product.Species = updatedProduct.Species ?? product.Species;

        await context.SaveChangesAsync();
        return product;
    }
    
    public async Task<bool> DeleteProduct(int id)
    {
        Product? product = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return false;
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return true;
    }
    
    public ProductResponse CreateProductResponse(Product product)
    {
        ProductResponse productResponse = new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Picture = product.Picture,
            Species = product.Species
        };

        return productResponse;
    }

    public RegisteredProductResponse CreateRegisteredProductResponse(RegisteredProduct registeredProduct)
    {
        if (registeredProduct.Product == null)
        {
            throw new NotFoundException("Product not found");
        }

        ProductResponse productResponse = CreateProductResponse(registeredProduct.Product);
        RegisteredProductResponse registeredProductResponse = new()
        {
            Id = registeredProduct.Id,
            Product = productResponse,
            MinPrice = registeredProduct.MinPrice,
            MaxPrice = registeredProduct.MaxPrice,
            Stock = registeredProduct.Stock,
            Region = registeredProduct.Region,
            HarvestedAt = registeredProduct.HarvestedAt,
            PotSize = registeredProduct.PotSize ?? null,
            StemLength = registeredProduct.StemLength ?? null,
            ProviderUserName = registeredProduct.User?.UserName ?? "Unknown"
        };

        return registeredProductResponse;
    }
}
