import {RegisteredProduct} from "@/types/Product/RegisteredProducts";

export interface Auction {
    id?: number;
    startDate: string;
    nextProductStartTime?: string;
    clockLocationEnum: number | null;
    registeredProducts: RegisteredProduct[];
    userId?: string;
    isLive?: boolean;
    isVisible?: boolean;
}