using LeafBidAPI.Data;
using LeafBidAPI.DTOs.AuctionSale;
using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.DTOs.Company;
using LeafBidAPI.DTOs.Product;
using LeafBidAPI.DTOs.RegisteredProduct;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Helpers;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace LeafBidAPI.Services;

public class ProductService(
    ApplicationDbContext context,
    UserManager<User> userManager,
    IAuctionSaleService auctionSaleService,
    IAuctionSaleProductService auctionSaleProductService,
    AuctionHelper auctionHelper
) : IProductService
{
    public async Task<List<Product>> GetProducts()
    {
        return await context.Products.ToListAsync();
    }

    public async Task<List<Product>> GetAvailableProducts()
    {
        List<Product> products = await context.Products
            .ToListAsync();

        return products;
    }

    public async Task<List<RegisteredProduct>> GetAvailableRegisteredProducts()
    {
        List<RegisteredProduct> registeredProducts = await context.RegisteredProducts
            .Include(rp => rp.Product)
            .Include(rp => rp.Company)
            .Where(rp => !context.AuctionProducts.Any(ap => ap.RegisteredProductId == rp.Id))
            .ToListAsync();

        return registeredProducts;
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
            .Where(rp => rp.Id == id)
            .Include(rp => rp.Product)
            .FirstOrDefaultAsync();

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

    public async Task<RegisteredProduct> CreateRegisteredProduct(
        CreateRegisteredProductEndpointDto registeredProductData, int productId, string userId, int companyId)
    {
        Product? product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }

        User? user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (user.CompanyId == null)
        {
            throw new NotFoundException("User is not part of a company");
        }

        RegisteredProduct registeredProduct = new()
        {
            ProductId = productId,
            MinPrice = registeredProductData.MinPrice,
            Stock = registeredProductData.Stock,
            Region = registeredProductData.Region,
            HarvestedAt = registeredProductData.HarvestedAt,
            StemLength = registeredProductData.StemLength,
            PotSize = registeredProductData.PotSize,
            CompanyId = companyId,
            UserId = userId
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
        ProductResponse? productResponse =
            registeredProduct.Product != null ? CreateProductResponse(registeredProduct.Product) : null;
        CompanyResponse? companyResponse = registeredProduct.Company != null
            ? new CompanyResponse
            {
                Id = registeredProduct.Company.Id,
                Name = registeredProduct.Company.Name,
                Street = registeredProduct.Company.Street,
                City = registeredProduct.Company.City,
                HouseNumber = registeredProduct.Company.HouseNumber,
                HouseNumberSuffix = registeredProduct.Company.HouseNumberSuffix ?? "",
                PostalCode = registeredProduct.Company.PostalCode,
                CountryCode = registeredProduct.Company.CountryCode,
            }
            : null;
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
            ProviderUserName = registeredProduct.User?.UserName ?? "Unknown",
            Company = companyResponse!,
        };

        return registeredProductResponse;
    }

    public async Task<AuctionEventResponse> BuyProduct(BuyProductDto buyData, string userId)
    {
        try
        {
            // Fetch auction to check pause and current start time
            Auction auction = await context.Auctions.FindAsync(buyData.AuctionId) ??
                              throw new NotFoundException("Auction not found");

            DateTime now = TimeHelper.GetAmsterdamTime();
            DateTime? startTime = auction.NextProductStartTime;

            if (startTime is null || now < startTime.Value)
            {
                throw new InvalidOperationException("Auction is currently paused.");
            }

            RegisteredProduct registeredProduct = await context.RegisteredProducts
                                                      .FirstOrDefaultAsync(rp =>
                                                          rp.Id == buyData.RegisteredProductId) ??
                                                  throw new NotFoundException("Registered product not found");

            if (registeredProduct.Stock < buyData.Quantity)
            {
                throw new InvalidOperationException("Not enough stock available in this auction");
            }

            // Calculate price on the server side
            decimal pricePerUnit = CalculateCurrentPrice(auction, registeredProduct);

            // Reduce stock
            registeredProduct.Stock -= buyData.Quantity;

            // Check if there are any products with stock > 0 left in this auction
            bool hasOtherProducts = await context.AuctionProducts
                .AnyAsync(ap => ap.AuctionId == auction.Id
                                && ap.RegisteredProductId != registeredProduct.Id
                                && ap.RegisteredProduct!.Stock > 0
                );

            bool currentProductHasStock = registeredProduct.Stock > 0;

            // If this is the last product and no more stock is available,
            // make it unIsLive itself
            if (!hasOtherProducts && !currentProductHasStock)
            {
                auction.IsLive = false;
            }

            // Reset auction timer for the next product
            auction.NextProductStartTime = TimeHelper.GetAmsterdamTime().AddSeconds(5);

            // Create an AuctionSale entry
            AuctionSale auctionSale = await auctionSaleService.CreateAuctionSale(new CreateAuctionSaleDto
            {
                AuctionId = auction.Id,
                UserId = userId,
                Date = TimeHelper.GetAmsterdamTime(),
                PaymentReference = "PAY-" + Guid.NewGuid().ToString()[..8].ToUpper()
            });

            // Create AuctionSaleProduct entry
            await auctionSaleProductService.CreateAuctionSaleProduct(new CreateAuctionSaleProductDto
            {
                AuctionSaleId = auctionSale.Id,
                RegisteredProductId = registeredProduct.Id,
                Quantity = buyData.Quantity,
                PricePerUnit = pricePerUnit
            });

            return new AuctionEventResponse
            {
                RegisteredProduct = registeredProduct,
                NextProductStartTime = auction.NextProductStartTime,
                IsSuccess = true
            };
        }
        catch (NotFoundException ex)
        {
            throw new NotFoundException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(ex.Message);
        }
    }

    public async Task<AuctionEventResponse> ExpireProduct(int registeredProductId, int auctionId)
    {
        try
        {
            // Fetch registered product and auction
            RegisteredProduct registeredProduct = await context.RegisteredProducts
                                                      .FirstOrDefaultAsync(rp => rp.Id == registeredProductId)
                                                  ?? throw new NotFoundException("Registered product not found");

            Auction auction = await context.Auctions.FindAsync(auctionId)
                              ?? throw new NotFoundException("Auction not found");

            registeredProduct.Stock = 0;

            // Check if there are any products with stock > 0 left in this auction
            bool hasOtherProducts = await context.AuctionProducts
                .AnyAsync(ap =>
                    ap.AuctionId == auction.Id
                    && ap.RegisteredProductId != registeredProduct.Id
                    && ap.RegisteredProduct!.Stock > 0
                );

            if (!hasOtherProducts)
            {
                auction.IsLive = false;
            }

            // Reset auction timer for the next product
            auction.NextProductStartTime = TimeHelper.GetAmsterdamTime().AddSeconds(5);

            await context.SaveChangesAsync();

            return new AuctionEventResponse
            {
                RegisteredProduct = registeredProduct,
                NextProductStartTime = auction.NextProductStartTime,
                IsSuccess = true
            };
        }
        catch (NotFoundException ex)
        {
            throw new NotFoundException(ex.Message);
        }
    }

    private decimal CalculateCurrentPrice(Auction auction, RegisteredProduct product)
    {
        if (auction.NextProductStartTime == null) return product.MaxPrice ?? product.MinPrice;

        double elapsedSeconds = (TimeHelper.GetAmsterdamTime() - auction.NextProductStartTime.Value).TotalSeconds;
        if (elapsedSeconds <= 0) return product.MaxPrice ?? product.MinPrice;

        decimal startPrice = product.MaxPrice ?? product.MinPrice;
        decimal minPrice = product.MinPrice;

        double durationSeconds = auctionHelper.GetProductDurationSeconds(product);
        if (durationSeconds <= 0) return minPrice;

        decimal startCents = startPrice * 100;
        decimal rangeCents = (startPrice - minPrice) * 100;

        decimal decreaseCentsPerSecond = rangeCents / (decimal)durationSeconds;
        decimal currentCents = startCents - (decreaseCentsPerSecond * (decimal)elapsedSeconds);

        return Math.Max(minPrice, Math.Ceiling(currentCents) / 100m);
    }
}