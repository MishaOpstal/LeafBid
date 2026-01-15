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
        public const string ManageRegistered = "products.manage.registered.policy";
    }
    
    public static class Companies
    {
        public const string View = "companies.view.policy";
        public const string Manage = "companies.manage.policy";
    }

    public static class Roles
    {
        public const string ViewOthers = "roles.others.view";
        public const string Manage = "roles.manage";
    }

    public static class Users
    {
        public const string ViewOthers = "users.others.view";
        public const string Manage = "users.manage";
    }
}