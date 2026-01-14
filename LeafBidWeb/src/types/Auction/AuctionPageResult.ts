import { Auction } from "@/types/Auction/Auction";
import { RegisteredProduct } from "@/types/Product/RegisteredProducts";

export interface AuctionPageResult {
    auction: Auction;
    registeredProducts: RegisteredProduct[];
    serverTime: string;
}
