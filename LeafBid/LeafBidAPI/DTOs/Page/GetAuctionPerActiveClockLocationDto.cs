namespace LeafBidAPI.DTOs.Page;

using Models;

public class GetAuctionPerActiveClockLocationDto
{
    public required List<GetAuctionWithProductsDto> Auctions { get; set; }
}