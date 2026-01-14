import {RegisteredProduct} from "@/types/Product/RegisteredProducts";
import {Product} from "@/types/Product/Product";
import {Company} from "@/types/Company/Company";

export interface AuctionSaleProduct {
    quantity: number;
    price: number;
    date: Date;
    registeredProduct: RegisteredProduct;
    product: Product;
    company: Company;
}