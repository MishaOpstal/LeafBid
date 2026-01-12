using LeafBidAPI.Data;
using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using LeafBidAPI.Enums;
using LeafBidAPI.Helpers;

namespace LeafBidAPI.Services;

public class AuctionSaleProductService(ApplicationDbContext context) : IAuctionSaleProductService
{
    public async Task<List<AuctionSaleProduct>> GetAuctionSaleProducts()
    {
        return await context.AuctionSaleProducts.ToListAsync();
    }

    public async Task<AuctionSaleProduct> GetAuctionSaleProductById(int id)
    {
        AuctionSaleProduct? auctionSaleProduct =
            await context.AuctionSaleProducts.FirstOrDefaultAsync(asp => asp.Id == id);

        if (auctionSaleProduct == null)
        {
            throw new NotFoundException("Auction sale product not found");
        }

        return auctionSaleProduct;
    }
    
public async Task<AuctionSaleProductHistoryResponse> GetAuctionSaleProductsHistory(
    int registeredProductId,
    HistoryEnum scope,
    bool includeCompanyName,
    int? limit = 10
)
{
    await using SqlConnection connection = (SqlConnection)context.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

    // Haal productId en companyId op
    int productId, companyId;
    const string productQuery = """
        SELECT ProductId, CompanyId
        FROM RegisteredProducts
        WHERE Id = @registeredProductId
        """;
    
    await using (SqlCommand productCmd = new SqlCommand(productQuery, connection))
    {
        productCmd.Parameters.AddWithValue("@registeredProductId", registeredProductId);
        await using var reader = await productCmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new InvalidOperationException("Registered product not found.");

        productId = reader.GetInt32(0);
        companyId = reader.GetInt32(1);
    }

    // Bepaal filter op basis van scope
    string companyFilter = scope switch
    {
        HistoryEnum.All => "",
        HistoryEnum.OnlyCompany => "AND rp.CompanyId = @companyId",
        HistoryEnum.ExcludeCompany => "AND rp.CompanyId <> @companyId",
        _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid history scope")
    };

    // Bereken gemiddelde prijs
    const string avgPriceQueryTemplate = """
        SELECT AVG(asp.Price)
        FROM AuctionSaleProducts asp
        JOIN RegisteredProducts rp ON asp.RegisteredProductId = rp.Id
        WHERE rp.ProductId = @productId
        {0}
        """;

    string avgPriceQuery = string.Format(avgPriceQueryTemplate, companyFilter);
    decimal averagePrice = 0;

    await using (var avgCmd = new SqlCommand(avgPriceQuery, connection))
    {
        avgCmd.Parameters.AddWithValue("@productId", productId);
        if (scope != HistoryEnum.All)
            avgCmd.Parameters.AddWithValue("@companyId", companyId);

        var result = await avgCmd.ExecuteScalarAsync();
        if (result != DBNull.Value)
            averagePrice = Convert.ToDecimal(result);
    }

    // Haal recente verkopen op
    string topClause = limit.HasValue ? $"TOP {limit}" : "";
    string historyQuery = $"""
        SELECT {topClause} 
            p.Name, p.Picture, asp.Quantity, asp.Price, a.Date
            {(includeCompanyName ? ", c.Name AS CompanyName" : "")}
        FROM AuctionSaleProducts asp
        JOIN AuctionSales a ON asp.AuctionSaleId = a.Id
        JOIN RegisteredProducts rp ON asp.RegisteredProductId = rp.Id
        JOIN Products p ON rp.ProductId = p.Id
        {(includeCompanyName ? "JOIN Companies c ON rp.CompanyId = c.Id" : "")}
        WHERE rp.ProductId = @productId
        {companyFilter}
        ORDER BY a.Date DESC
        """;

    var recentSales = new List<AuctionSaleProductHistorySalesDto>();
    await using (var cmd = new SqlCommand(historyQuery, connection))
    {
        cmd.Parameters.AddWithValue("@productId", productId);
        if (scope != HistoryEnum.All)
            cmd.Parameters.AddWithValue("@companyId", companyId);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            recentSales.Add(new AuctionSaleProductHistorySalesDto()
            {
                Quantity = reader.GetInt32(2),
                Price = reader.GetDecimal(3),
                Date = reader.GetDateTime(4),
                CompanyName = includeCompanyName && !reader.IsDBNull(5) ? reader.GetString(5) : null
            });
        }
    }

    return new AuctionSaleProductHistoryResponse
    {
        AvgPrice = averagePrice,
        RecentSales = recentSales
    };
}


public async Task<List<AuctionSaleProductResponse>> GetAuctionSaleProductsByUserId(string userId)
    {
        List<AuctionSaleProduct> list = await context.AuctionSaleProducts
            .Where(asp => asp.AuctionSale != null && asp.AuctionSale.UserId == userId)
            .Include(auctionSaleProduct => auctionSaleProduct.RegisteredProduct)
            .ThenInclude(rp => rp!.Product)
            .Include(auctionSaleProduct => auctionSaleProduct.AuctionSale)
            .ToListAsync();
        List<AuctionSaleProductResponse> responseList = list.Select(asp => new AuctionSaleProductResponse
        {
            Name = asp.RegisteredProduct?.Product?.Name ?? "Unknown Product",
            Picture = asp.RegisteredProduct?.Product?.Picture ?? string.Empty,
            Price = asp.Price,
            Quantity = asp.Quantity,
            Date = asp.AuctionSale?.Date ?? DateTime.MinValue
        }).ToList();
        return responseList;
    }

    public async Task<List<AuctionSaleProductResponse>> GetAuctionSaleProductsByCompanyId(int companyId)
    {
        var responseList = await context.AuctionSaleProducts
            .Where(asp => asp.RegisteredProduct != null && asp.RegisteredProduct.CompanyId == companyId)
            .GroupBy(asp => new
            {
                asp.RegisteredProductId,
                asp.AuctionSale.AuctionId,
                ProductName = asp.RegisteredProduct.Product.Name,
                ProductPicture = asp.RegisteredProduct.Product.Picture
            })
            .Select(g => new AuctionSaleProductResponse
            {
                Name = g.Key.ProductName ?? "Unknown Product",
                Picture = g.Key.ProductPicture ?? string.Empty,
                Price = g.Sum(x => x.Price),
                Quantity = g.Sum(x => x.Quantity),
                Date = g.Max(x => x.AuctionSale.Date)
            })
            .ToListAsync();

        return responseList;
    }
    
    
    public async Task<SaleChartResponse> GetSaleChartDataByCompany(int companyId)
    {
        var data = await context.AuctionSaleProducts
            .Where(asp => asp.RegisteredProduct.CompanyId == companyId)
            .Include(asp => asp.RegisteredProduct)
            .ThenInclude(rp => rp.Product)
            .Include(asp => asp.AuctionSale)
            .ToListAsync();
        
        var currentMonthData = data
            .Where(asp => asp.AuctionSale.Date.Month == DateTime.Now.Month 
                          && asp.AuctionSale.Date.Year == DateTime.Now.Year)
            .GroupBy(asp => new 
            { 
                asp.RegisteredProduct.Product.Id, 
                asp.RegisteredProduct.Product.Name 
            })
            .Select(g => new SaleChartDataPoint
            {
                ProductName = g.Key.Name,
                Price = g.Sum(x => x.Price)
            })
            .ToList();

        var allTimeData = data
            .GroupBy(asp => new 
            { 
                asp.RegisteredProduct.Product.Id, 
                asp.RegisteredProduct.Product.Name 
            })
            .Select(g => new SaleChartDataPoint
            {
                ProductName = g.Key.Name,
                Price = g.Sum(x => x.Price)
            })
            .ToList();


        return new SaleChartResponse
        {
            CurrentMonthData = currentMonthData,
            AllTimeData = allTimeData
        };
    }


    
    public async Task<AuctionSaleProduct> CreateAuctionSaleProduct(
        CreateAuctionSaleProductDto auctionSaleProductData)
    {
        AuctionSaleProduct auctionSaleProduct = new()
        {
            AuctionSaleId = auctionSaleProductData.AuctionSaleId,
            RegisteredProductId = auctionSaleProductData.ProductId,
            Quantity = auctionSaleProductData.Quantity,
            Price = auctionSaleProductData.Price
        };

        context.AuctionSaleProducts.Add(auctionSaleProduct);
        await context.SaveChangesAsync();

        return auctionSaleProduct;
    }
    
    public async Task<AuctionSaleProduct> UpdateAuctionSaleProduct(
        int id,
        UpdateAuctionSaleProductDto auctionSaleProductData)
    {
        AuctionSaleProduct? auctionSaleProducts =
            await context.AuctionSaleProducts.FirstOrDefaultAsync(asp => asp.Id == id);

        if (auctionSaleProducts == null)
        {
            throw new NotFoundException("Auction sale product not found");
        }

        auctionSaleProducts.AuctionSaleId = auctionSaleProductData.AuctionSaleId;
        auctionSaleProducts.RegisteredProductId = auctionSaleProductData.ProductId;
        auctionSaleProducts.Quantity = auctionSaleProductData.Quantity;
        auctionSaleProducts.Price = auctionSaleProductData.Price;

        await context.SaveChangesAsync();

        return auctionSaleProducts;
    }

    public async Task<AuctionEventResponse> BuyProduct(BuyProductDto buyData, string userId)
    {
        // Fetch auction to check pause and current start time
        Auction? auction = await context.Auctions.FindAsync(buyData.AuctionId);
        if (auction == null)
        {
            throw new NotFoundException("Auction not found");
        }

        if (auction.NextProductStartTime == null || TimeHelper.GetAmsterdamTime() < auction.NextProductStartTime)
        {
            throw new InvalidOperationException("Auction is currently paused.");
        }

        RegisteredProduct? registeredProduct = await context.RegisteredProducts
            .FirstOrDefaultAsync(rp => rp.Id == buyData.RegisteredProductId);

        if (registeredProduct == null)
        {
            throw new NotFoundException("Registered product not found");
        }

        AuctionProduct? auctionProduct = await context.AuctionProducts
            .FirstOrDefaultAsync(ap => ap.AuctionId == auction.Id && ap.RegisteredProductId == registeredProduct.Id);

        if (auctionProduct == null || auctionProduct.AuctionStock < buyData.Quantity)
        {
            throw new InvalidOperationException("Not enough stock available in this auction");
        }

        if (registeredProduct.Stock < buyData.Quantity)
        {
            throw new InvalidOperationException("Not enough total stock available");
        }

        // Calculate price on server side
        decimal pricePerUnit = CalculateCurrentPrice(auction, registeredProduct);

        // Reduce stock
        registeredProduct.Stock -= buyData.Quantity;
        
        if (auctionProduct != null)
        {
            auctionProduct.AuctionStock -= buyData.Quantity;
        }

        // Check if there are any products with stock > 0 left in this auction
        bool hasOtherProducts = await context.AuctionProducts
            .AnyAsync(ap => ap.AuctionId == auction.Id && ap.RegisteredProductId != registeredProduct.Id && ap.AuctionStock > 0);

        bool currentProductHasStock = auctionProduct?.AuctionStock > 0;

        if (!hasOtherProducts && !currentProductHasStock)
        {
            auction.IsLive = false;
        }

        // Reset auction timer for the next product or same product if stock remains
        auction.NextProductStartTime = TimeHelper.GetAmsterdamTime().AddSeconds(5);

        // Create an AuctionSale entry
        AuctionSale auctionSale = new()
        {
            AuctionId = buyData.AuctionId,
            UserId = userId,
            Date = TimeHelper.GetAmsterdamTime(),
            PaymentReference = "PAY-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
        };

        context.AuctionSales.Add(auctionSale);
        await context.SaveChangesAsync();

        // Create AuctionSaleProduct entry
        AuctionSaleProduct auctionSaleProduct = new()
        {
            AuctionSaleId = auctionSale.Id,
            RegisteredProductId = registeredProduct.Id,
            Quantity = buyData.Quantity,
            Price = pricePerUnit
        };

        context.AuctionSaleProducts.Add(auctionSaleProduct);
        await context.SaveChangesAsync();

        return new AuctionEventResponse
        {
            RegisteredProduct = registeredProduct,
            NextProductStartTime = auction.NextProductStartTime,
            IsSuccess = true
        };
    }

    public async Task<AuctionEventResponse> ExpireProduct(int registeredProductId, int auctionId)
    {
        RegisteredProduct? registeredProduct = await context.RegisteredProducts
            .FirstOrDefaultAsync(rp => rp.Id == registeredProductId);

        if (registeredProduct == null)
        {
            throw new NotFoundException("Registered product not found");
        }

        Auction? auction = await context.Auctions.FindAsync(auctionId);
        if (auction == null)
        {
            throw new NotFoundException("Auction not found");
        }

        // Validate if the product should actually expire
        double duration = AuctionHelper.GetProductDurationSeconds(registeredProduct);
        double elapsed = (TimeHelper.GetAmsterdamTime() - (auction.NextProductStartTime ?? auction.StartDate)).TotalSeconds;

        // 5 second tolerance for clock drift
        if (elapsed < duration - 5)
        {
            // Not expired yet, ignore request but return current state
            return new AuctionEventResponse
            {
                RegisteredProduct = registeredProduct,
                NextProductStartTime = null,
                IsSuccess = false
            };
        }

        AuctionProduct? auctionProduct = await context.AuctionProducts
            .FirstOrDefaultAsync(ap => ap.AuctionId == auction.Id && ap.RegisteredProductId == registeredProduct.Id);
        
        if (auctionProduct != null)
        {
            auctionProduct.AuctionStock = 0;
        }

        // Check if there are any products with stock > 0 left in this auction
        bool hasOtherProducts = await context.AuctionProducts
            .AnyAsync(ap => ap.AuctionId == auction.Id && ap.RegisteredProductId != registeredProduct.Id && ap.AuctionStock > 0);

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

    private decimal CalculateCurrentPrice(Auction auction, RegisteredProduct product)
    {
        if (auction.NextProductStartTime == null) return product.MaxPrice ?? product.MinPrice;
        
        double elapsedSeconds = (TimeHelper.GetAmsterdamTime() - auction.NextProductStartTime.Value).TotalSeconds;
        if (elapsedSeconds <= 0) return product.MaxPrice ?? product.MinPrice;

        decimal startPrice = product.MaxPrice ?? product.MinPrice;
        decimal minPrice = product.MinPrice;
        
        double durationSeconds = AuctionHelper.GetProductDurationSeconds(product);
        if (durationSeconds <= 0) return minPrice;

        decimal startCents = startPrice * 100;
        decimal rangeCents = (startPrice - minPrice) * 100;

        decimal decreaseCentsPerSecond = rangeCents / (decimal)durationSeconds;
        decimal currentCents = startCents - (decreaseCentsPerSecond * (decimal)elapsedSeconds);

        return Math.Max(minPrice, Math.Ceiling(currentCents) / 100m);
    }

}
