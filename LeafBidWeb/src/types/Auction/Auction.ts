import {Product} from "@/types/Product/Product";

export interface Auction {
    id?: number;
    startDate: string;
    nextProductStartTime?: string;
    clockLocationEnum: number;
    products: Product[]; // product(s) zitten hier in
    userId: string;
    isLive: boolean;
    isVisible: boolean;
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