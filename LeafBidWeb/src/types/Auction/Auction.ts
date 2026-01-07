import {Product} from "@/types/Product/Product";
import {RegisteredProduct} from "@/types/Product/RegisteredProducts";

export interface Auction {
    id?: number;
    startDate: string;
    clockLocationEnum: number;
    registeredProducts: RegisteredProduct[]; // product(s) zitten hier in
    userId: string;
    isLive: boolean;
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