import { Auction } from "@/types/Auction/Auction";
import { RegisteredProduct } from "@/types/Product/RegisteredProducts";

export interface AuctionPage {
    auction: Auction;
    registeredProducts: RegisteredProduct[];
}
