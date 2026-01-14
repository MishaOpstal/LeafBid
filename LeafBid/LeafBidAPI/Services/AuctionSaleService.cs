using LeafBidAPI.Data;
using LeafBidAPI.DTOs.AuctionSale;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Services;

public class AuctionSaleService(
    ApplicationDbContext context
) : IAuctionSaleService
{
    public async Task<List<AuctionSale>> GetAuctionSales()
    {
        return await context.AuctionSales.ToListAsync();
    }

    public async Task<AuctionSale> GetAuctionSaleById(int id)
    {
        AuctionSale? auctionSale =
            await context.AuctionSales.FirstOrDefaultAsync(sale => sale.Id == id);

        if (auctionSale == null)
        {
            throw new NotFoundException("Auction sale not found");
        }

        return auctionSale;
    }

    public async Task<AuctionSale> CreateAuctionSale(CreateAuctionSaleDto auctionSaleData)
    {
        AuctionSale auctionSale = new()
        {
            AuctionId = auctionSaleData.AuctionId,
            UserId = auctionSaleData.UserId,
            PaymentReference = auctionSaleData.PaymentReference,
            Date = auctionSaleData.Date
        };

        context.AuctionSales.Add(auctionSale);
        await context.SaveChangesAsync();

        return auctionSale;
    }
}