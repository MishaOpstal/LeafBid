import { Product } from "@/types/Product/Product";

export interface RegisteredProduct {
    id?: number;
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
}

export function parsePrice(price: number): string {
    return new Intl.NumberFormat('nl-NL', {
        style: 'currency',
        currency: 'EUR',
    }).format(price);
}

export function parseDate(date: Date|string): string {
    const options: Intl.DateTimeFormatOptions = {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: 'numeric',
        minute: 'numeric',
    };

    const formatter = new Intl.DateTimeFormat('nl-NL', options);
    return formatter.format(new Date(date));
}