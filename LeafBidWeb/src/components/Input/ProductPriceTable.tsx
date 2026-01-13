import React, {useCallback} from "react";
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
    // Safely update price per product
    const handlePriceChange = useCallback(
        (registeredProductId: number, newValue: number) => {
            const parsed = parseFloat(String(newValue));
            const price = isNaN(parsed) || parsed < 0 ? 0 : parsed;

            const updated = registeredProducts.map((rp) =>
                rp.id === registeredProductId ? {...rp, maxPrice: price} : rp
            );

            onChange?.(updated);
        },
        [registeredProducts, onChange]
    );

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
                    registeredProducts.map((registeredProduct) => (
                        <tr key={registeredProduct.id}>
                            <td className={s.productName}>
                                {registeredProduct.product.name}
                            </td>
                            <td className={s.priceCell}>
                                <Form.Control
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    value={registeredProduct.maxPrice?.toString() ?? ""}
                                    placeholder="0.00"
                                    onChange={(e) => {
                                        // Allow typing any valid number (still sends numeric to parent)
                                        const val = parseFloat(e.target.value);
                                        handlePriceChange(registeredProduct.id, parseFloat(val.toFixed(2)) || 0);
                                    }}
                                    onBlur={(e) => {
                                        // Format to 2 decimals only when leaving the field
                                        const val = parseFloat(e.target.value);
                                        if (!isNaN(val)) {
                                            handlePriceChange(registeredProduct.id, parseFloat(val.toFixed(2)));
                                        }
                                    }}
                                    className={s.priceInput}
                                />
                            </td>
                        </tr>
                    ))
                ) : (
                    <tr>
                        <td
                            colSpan={2}
                            className="text-center text-muted py-3"
                        >
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
