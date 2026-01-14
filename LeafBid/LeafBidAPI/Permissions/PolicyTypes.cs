namespace LeafBidAPI.Permissions;

public static class PolicyTypes
{
    public static class Auctions
    {
        public const string View = "auctions.view.policy";
        public const string Manage = "auctions.manage.policy";
    }

    public static class AuctionSales
    {
        public const string View = "sales.view.policy";
    }
    
    public static class Products
    {
        public const string View = "products.view.policy";
        public const string Buy = "products.buy.policy";
        public const string Manage = "products.manage.policy";
    }
}