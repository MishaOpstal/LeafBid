import React, {useCallback, useEffect, useState} from "react";
import {Form, Table} from "react-bootstrap";
import s from "./ProductPriceTable.module.css";
import {RegisteredProduct} from "@/types/Product/RegisteredProducts";

interface ProductPriceTableProps {
    registeredProducts: RegisteredProduct[]; // Array of products with productId, productName, etc.
    onChange?: (updated: RegisteredProduct[]) => void; // Fired when a price changes
    height?: number | string; // Optional scrollable height
}

/**
 * A controlled, scrollable table that lets the User assign prices
 * to products. Each row includes the product name (read-only)
 * and a numeric Input for its price.
 */
const ProductPriceTable: React.FC<ProductPriceTableProps> = ({
                                                                 registeredProducts,
                                                                 onChange,
                                                                 height = 300,
                                                             }) => {
    // Local state for input values during editing to avoid constant parent updates
    const [localPrices, setLocalPrices] = useState<Record<number, string>>({});

    // Handle input change - updates local state only
    const handleInputChange = useCallback((registeredProductId: number, value: string) => {
        setLocalPrices(prev => ({
            ...prev,
            [registeredProductId]: value
        }));
    }, []);

    // Handle blur - validates, formats, and updates parent
    const handleBlur = useCallback((registeredProductId: number, value: string) => {
        if (!onChange) return;

        const registeredProduct = registeredProducts.find(rp => rp.id === registeredProductId);
        if (!registeredProduct) return;

        const parsed = parseFloat(value);
        let price = isNaN(parsed) || parsed < 0 ? 0 : parseFloat(parsed.toFixed(2));

        // Ensure the price is not below minPrice
        if (registeredProduct.minPrice !== undefined && price < registeredProduct.minPrice) {
            price = registeredProduct.minPrice;
        }

        // Update parent only when the user finishes editing
        const updated = registeredProducts.map((rp) =>
            rp.id === registeredProductId ? {...rp, maxPrice: price} : rp
        );

        onChange(updated);

        // Clear the local state for this field
        setLocalPrices(prev => {
            const next = {...prev};
            delete next[registeredProductId];
            return next;
        });
    }, [onChange, registeredProducts]);

    // For every registered product, make sure the default maxPrice is equal to the minPrice
    useEffect(() => {
        if (!onChange) {
            return;
        }

        const needsInit = registeredProducts.some((rp) => rp.maxPrice == null);
        if (!needsInit) return;

        const updated = registeredProducts.map((rp) => {
            const min = rp.minPrice ?? 0;
            const defaultMax = min + 0.01;

            return rp.maxPrice == null ? { ...rp, maxPrice: defaultMax } : rp;
        });

        onChange(updated);
    }, [registeredProducts, onChange]);

    return (
        <div className={s.wrapper} style={{maxHeight: height}}>
            <Table striped size="sm" className={s.table}>
                <thead className={s.header}>
                <tr>
                    <th style={{width: "70%"}} className={s.th}>Productnaam</th>
                    <th style={{width: "30%"}} className={s.th}>Prijs (€)</th>
                </tr>
                </thead>
                <tbody>
                {registeredProducts.length > 0 ? (
                    registeredProducts.map((registeredProduct) => {
                        const minPrice = registeredProduct.minPrice ?? 0;
                        const minAllowedPrice = minPrice + 0.01;

                        const fallbackMaxPrice = registeredProduct.maxPrice ?? minAllowedPrice;

                        const value =
                            localPrices[registeredProduct.id] ??
                            fallbackMaxPrice.toFixed(2);

                        return (
                            <tr key={registeredProduct.id}>
                                <td className={s.productName}>
                                    {registeredProduct.product!.name}
                                </td>
                                <td className={s.priceCell}>
                                    <Form.Control
                                        type="number"
                                        step="0.01"
                                        min={minAllowedPrice}
                                        value={value}
                                        placeholder="0.00"
                                        onChange={(e) =>
                                        {
                                            handleInputChange(registeredProduct.id, e.target.value);
                                        }}
                                        onBlur={(e) =>
                                        {
                                            handleBlur(registeredProduct.id, e.target.value);
                                        }}
                                        className={s.priceInput}
                                    />
                                </td>
                            </tr>
                        );
                    })
                ) : (
                    <tr>
                        <td colSpan={2} className="text-center text-muted py-3">
                            Geen producten geselecteerd
                        </td>
                    </tr>
                )}
                </tbody>
            </Table>
        </div>
    );
};

export default ProductPriceTable;