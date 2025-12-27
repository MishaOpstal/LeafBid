import React, { useCallback } from "react";
import { Table, Form } from "react-bootstrap";
import { Product } from "@/types/Product/Product";
import s from "./ProductPriceTable.module.css";

interface ProductPriceTableProps {
    products: Product[]; // Array of products with productId, productName, etc.
    onChange?: (updated: Product[]) => void; // Fired when a price changes
    height?: number | string; // Optional scrollable height
}

/**
 * A controlled, scrollable table that lets the User assign prices
 * to products. Each row includes the product name (read-only)
 * and a numeric input for its price.
 */
const ProductPriceTable: React.FC<ProductPriceTableProps> = ({
                                                                 products,
                                                                 onChange,
                                                                 height = 300,
                                                             }) => {
    // Safely update price per product
    const handlePriceChange = useCallback(
        (productId: number, newValue: number) => {
            const parsed = parseFloat(String(newValue));
            const price = isNaN(parsed) || parsed < 0 ? 0 : parsed;

            const updated = products.map((p) =>
                p.id === productId ? { ...p, maxPrice: price } : p
            );

            onChange?.(updated);
        },
        [products, onChange]
    );

    return (
        <div className={s.wrapper} style={{ maxHeight: height }}>
            <Table striped bordered hover size="sm" className={s.table}>
                <thead className={s.header}>
                <tr>
                    <th style={{ width: "70%" }} className={s.th}>Productnaam</th>
                    <th style={{ width: "30%" }} className={s.th}>Prijs (€)</th>
                </tr>
                </thead>
                <tbody>
                {products.length > 0 ? (
                    products.map((product) => (
                        <tr key={product.id}>
                            <td className={s.productName}>
                                {product.name}
                            </td>
                            <td className={s.priceCell}>
                                <Form.Control
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    value={product.maxPrice?.toString() ?? ""}
                                    placeholder="0.00"
                                    onChange={(e) => {
                                        // Allow typing any valid number (still sends numeric to parent)
                                        const val = parseFloat(e.target.value);
                                        handlePriceChange(product.id, parseFloat(val.toFixed(2)) || 0);
                                    }}
                                    onBlur={(e) => {
                                        // Format to 2 decimals only when leaving the field
                                        const val = parseFloat(e.target.value);
                                        if (!isNaN(val)) {
                                            handlePriceChange(product.id, parseFloat(val.toFixed(2)));
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
