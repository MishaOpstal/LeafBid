export enum Roles {
    Buyer,
    Provider,
    Auctioneer,
    Admin
}

export function parseRole(role: number) {
    switch (role) {
        case Roles.Buyer:
            return "Buyer";
        case Roles.Provider:
            return "Provider";
        case Roles.Auctioneer:
            return "Auctioneer";
        case Roles.Admin:
            return "Admin";
        default:
            return "Unknown";
    }
}