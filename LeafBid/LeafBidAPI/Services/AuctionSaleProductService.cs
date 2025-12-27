using LeafBidAPI.Data;
using LeafBidAPI.DTOs.AuctionSaleProduct;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
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
    
    public async Task<AuctionSaleProduct> CreateAuctionSaleProduct(
        CreateAuctionSaleProductDto auctionSaleProductData)
    {
        AuctionSaleProduct auctionSaleProduct = new()
        {
            AuctionSaleId = auctionSaleProductData.AuctionSaleId,
            ProductId = auctionSaleProductData.ProductId,
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
        auctionSaleProducts.ProductId = auctionSaleProductData.ProductId;
        auctionSaleProducts.Quantity = auctionSaleProductData.Quantity;
        auctionSaleProducts.Price = auctionSaleProductData.Price;

        await context.SaveChangesAsync();

        return auctionSaleProducts;
    }
}
