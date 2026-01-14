using System.Data;
using LeafBidAPI.Data;
using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.Enums;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Services;

public class AuctionSaleProductService(ApplicationDbContext context)
    : IAuctionSaleProductService
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
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        // Haal productId en companyId op
        int productId, companyId;
        const string productQuery = """
                                    SELECT ProductId, CompanyId
                                    FROM RegisteredProducts
                                    WHERE Id = @registeredProductId
                                    """;

        await using (SqlCommand productCmd = new(productQuery, connection))
        {
            productCmd.Parameters.AddWithValue("@registeredProductId", registeredProductId);
            await using SqlDataReader? reader = await productCmd.ExecuteReaderAsync();
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

        await using (SqlCommand avgCmd = new(avgPriceQuery, connection))
        {
            avgCmd.Parameters.AddWithValue("@productId", productId);
            if (scope != HistoryEnum.All)
                avgCmd.Parameters.AddWithValue("@companyId", companyId);

            object? result = await avgCmd.ExecuteScalarAsync();
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

        List<AuctionSaleProductHistorySalesDto> recentSales = [];
        await using (SqlCommand cmd = new(historyQuery, connection))
        {
            cmd.Parameters.AddWithValue("@productId", productId);
            if (scope != HistoryEnum.All)
                cmd.Parameters.AddWithValue("@companyId", companyId);

            await using SqlDataReader? reader = await cmd.ExecuteReaderAsync();
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
        List<AuctionSaleProductResponse> responseList = await context.AuctionSaleProducts
            .Where(asp => asp.RegisteredProduct != null && asp.RegisteredProduct.CompanyId == companyId)
            .GroupBy(asp => new
            {
                asp.RegisteredProductId,
                asp.AuctionSale!.AuctionId,
                ProductName = asp.RegisteredProduct!.Product!.Name,
                ProductPicture = asp.RegisteredProduct.Product.Picture
            })
            .Select(g => new AuctionSaleProductResponse
            {
                Name = g.Key.ProductName,
                Picture = g.Key.ProductPicture ?? string.Empty,
                Price = g.Sum(x => x.Price),
                Quantity = g.Sum(x => x.Quantity),
                Date = g.Max(x => x.AuctionSale!.Date)
            })
            .ToListAsync();

        return responseList;
    }


    public async Task<SaleChartResponse> GetSaleChartDataByCompany(int companyId)
    {
        List<AuctionSaleProduct> data = await context.AuctionSaleProducts
            .Where(asp => asp.RegisteredProduct!.CompanyId == companyId)
            .Include(asp => asp.RegisteredProduct)
            .ThenInclude(rp => rp!.Product)
            .Include(asp => asp.AuctionSale)
            .ToListAsync();

        List<SaleChartDataPoint> currentMonthData = data
            .Where(asp => asp.AuctionSale!.Date.Month == DateTime.Now.Month
                          && asp.AuctionSale.Date.Year == DateTime.Now.Year)
            .GroupBy(asp => new
            {
                asp.RegisteredProduct!.Product!.Id,
                asp.RegisteredProduct.Product.Name
            })
            .Select(g => new SaleChartDataPoint
            {
                ProductName = g.Key.Name,
                Price = g.Sum(x => x.Price)
            })
            .ToList();

        List<SaleChartDataPoint> allTimeData = data
            .GroupBy(asp => new
            {
                asp.RegisteredProduct!.Product!.Id,
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
            RegisteredProductId = auctionSaleProductData.RegisteredProductId,
            Quantity = auctionSaleProductData.Quantity,
            Price = auctionSaleProductData.PricePerUnit
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
}