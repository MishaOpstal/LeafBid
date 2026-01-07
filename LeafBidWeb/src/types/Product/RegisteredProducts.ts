import { Product } from "@/types/Product/Product";
import {Company} from "@/types/Company/Company";

export interface RegisteredProduct {
    id: number;
    product: Product;
    minPrice: number;
    maxPrice: number;
    stock: number;
    region: string;
    harvestedAt: string;
    potSize?: number | null;
    stemLength?: number;
    providerUserName?: string;
    auctionId?: number;
    company: Company;
}
