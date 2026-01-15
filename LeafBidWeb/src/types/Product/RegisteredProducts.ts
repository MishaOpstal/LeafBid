import {Product} from "@/types/Product/Product";
import {Company} from "@/types/Company/Company";

export interface RegisteredProduct {
    id: number;
    productId: number;
    product?: Product;
    minPrice: number;
    maxPrice: number;
    stock: number;
    region: string;
    harvestedAt: string;
    potSize?: number | null;
    stemLength?: number;
    providerUserName?: string;
    auctionId?: number;
    companyId: number;
    company?: Company;
}

export interface RegisteredProductForAuction {
    id: number;
    maxPrice: number;
}

export function parsePrice(price: number): string {
    return new Intl.NumberFormat('nl-NL', {
        style: 'currency',
        currency: 'EUR',
    }).format(price);
}