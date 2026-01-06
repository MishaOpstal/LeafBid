using LeafBidAPI.Data;
using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
    
        public async Task<List<AuctionSaleProductHistoryResponse>> GetAuctionSaleProductsHistory(int registeredProductId)
    {
        const string sqlAvgPrice = """
                           SELECT 
                               AVG(asp.Price) AS AveragePrice
                           FROM AuctionSaleProducts asp
                           JOIN AuctionSales a ON asp.AuctionSaleId = a.Id
                           JOIN RegisteredProducts rp ON asp.RegisteredProductId = rp.Id
                           WHERE 
                               rp.ProductId = (SELECT ProductId FROM RegisteredProducts WHERE Id = @registeredProductId)
                           """;
        
        const string sqlHistory = """
                           SELECT TOP 10
                               p.Name,
                               p.Picture,
                               asp.Quantity,
                               asp.Price,
                               a.Date,
                               c.Name AS CompanyName
                           FROM AuctionSaleProducts asp
                           JOIN AuctionSales a ON asp.AuctionSaleId = a.Id
                           JOIN RegisteredProducts rp ON asp.RegisteredProductId = rp.Id
                           JOIN Products p ON rp.ProductId = p.Id
                            JOIN Companies c ON rp.CompanyId = c.Id
                           WHERE 
                               rp.ProductId = (SELECT ProductId FROM RegisteredProducts WHERE Id = @registeredProductId)
                               ORDER BY a.Date DESC
                           """;

        await using SqlConnection connection = (SqlConnection)context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        
        await using SqlCommand avgPriceCommand = new SqlCommand(sqlAvgPrice, connection);
        avgPriceCommand.Parameters.AddWithValue("@registeredProductId", registeredProductId);
        decimal averagePrice = 0;
        await using SqlDataReader? avgPriceReader = await avgPriceCommand.ExecuteReaderAsync();
        while (await avgPriceReader.ReadAsync())
        {
            if (!avgPriceReader.IsDBNull(0))
            {
                averagePrice = avgPriceReader.GetDecimal(0);
            }
        }
        avgPriceReader.Close();

        
        await using SqlCommand command = new SqlCommand(sqlHistory, connection);
        command.Parameters.AddWithValue("@registeredProductId", registeredProductId);
        List<AuctionSaleProductResponse> result = [];
        
        await using SqlDataReader? reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new AuctionSaleProductResponse
            {
                Name = reader.GetString(0),
                Picture = reader.GetString(1),
                Quantity = reader.GetInt32(2),
                Price = reader.GetDecimal(3),
                Date = reader.GetDateTime(4),
                CompanyName = reader.GetString(5)
            });
        }
        
        List<AuctionSaleProductHistoryResponse> responseList =
        [
            new AuctionSaleProductHistoryResponse
            {
                AvgPrice = averagePrice,
                RecentSales = result
            }
        ];
         return responseList;
    }
    public async Task<List<AuctionSaleProductResponse>> GetAuctionSaleProductsHistoryNotCompany(int registeredProductId)
    {
        const string sql = """
                           SELECT 
                               p.Name,
                               p.Picture,
                               asp.Quantity,
                               asp.Price,
                               a.Date,
                               c.Name AS CompanyName
                           FROM AuctionSaleProducts asp
                           JOIN AuctionSales a ON asp.AuctionSaleId = a.Id
                           JOIN RegisteredProducts rp ON asp.RegisteredProductId = rp.Id
                           JOIN Products p ON rp.ProductId = p.Id
                            JOIN Companies c ON rp.CompanyId = c.Id
                           WHERE 
                               rp.ProductId = (SELECT ProductId FROM RegisteredProducts WHERE Id = @registeredProductId)
                               AND
                               rp.CompanyId <> (SELECT CompanyId FROM RegisteredProducts WHERE Id = @registeredProductId)
                           """;

        await using SqlConnection connection = (SqlConnection)context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        await using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@registeredProductId", registeredProductId);
        List<AuctionSaleProductResponse> result = [];
        
        await using SqlDataReader? reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new AuctionSaleProductResponse
            {
                Name = reader.GetString(0),
                Picture = reader.GetString(1),
                Quantity = reader.GetInt32(2),
                Price = reader.GetDecimal(3),
                Date = reader.GetDateTime(4),
                CompanyName = reader.GetString(5)
            });
        }
        return result;
    }
    public async Task<List<AuctionSaleProductResponse>> GetAuctionSaleProductsHistoryCompany(int registeredProductId)
    {
        const string sql = """
                           SELECT 
                               p.Name,
                               p.Picture,
                               asp.Quantity,
                               asp.Price,
                               a.Date
                           FROM AuctionSaleProducts asp
                           JOIN AuctionSales a ON asp.AuctionSaleId = a.Id
                           JOIN RegisteredProducts rp ON asp.RegisteredProductId = rp.Id
                           JOIN Products p ON rp.ProductId = p.Id
                           WHERE 
                               rp.ProductId = (SELECT ProductId FROM RegisteredProducts WHERE Id = @registeredProductId)
                               AND
                               rp.CompanyId = (SELECT CompanyId FROM RegisteredProducts WHERE Id = @registeredProductId)
                           """;

        await using SqlConnection connection = (SqlConnection)context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        await using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@registeredProductId", registeredProductId);
        List<AuctionSaleProductResponse> result = new List<AuctionSaleProductResponse>();

        await using SqlDataReader? reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new AuctionSaleProductResponse
            {
                Name = reader.GetString(0),
                Picture = reader.GetString(1),
                Quantity = reader.GetInt32(2),
                Price = reader.GetDecimal(3),
                Date = reader.GetDateTime(4)
            });
        }

        return result;
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
}
