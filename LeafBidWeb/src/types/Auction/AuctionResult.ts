import { Auction } from "@/types/Auction/Auction";
import { RegisteredProduct } from "@/types/Product/RegisteredProducts";

export interface AuctionResult {
    auction: Auction;
    registeredProducts: RegisteredProduct[];
}
