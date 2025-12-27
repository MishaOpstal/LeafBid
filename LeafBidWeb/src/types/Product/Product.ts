export interface Product {
    id: number;
    name: string;
    description?: string;
    minPrice?: number;
    maxPrice?: number;
    weight?: number;
    picture?: string;
    species?: string;
    region?: string;
    potSize?: number;
    stemLength?: number;
    stock?: number;
    harvestedAt?: Date;
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