using LeafBidAPI.DTOs.RegisteredProduct;

namespace LeafBidAPI.DTOs.Page;
using Models;

public class GetAuctionWithProductsResponse
{
    public required Auction Auction { get; set; }

    public required List<RegisteredProductResponse> RegisteredProducts { get; set; }
}