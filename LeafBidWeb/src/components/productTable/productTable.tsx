"use client";
import Table from 'react-bootstrap/Table'
import {useEffect, useState} from "react";
interface Product {
    imageUrl: string;
    name: string;
    quantity: number;
    price: number;
    date: string;
}
function formatDate(dateStr: string) {
    const d = new Date(dateStr);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${day}-${month}-${year} | ${hours}:${minutes}`;
}

export default function ProductTable() {
    const [products, setProducts] = useState([]);
    useEffect(() => {
        const fetchProducts = async () => {
            try {
                const res = await fetch("http://localhost:5001/api/v2/AuctionSaleProduct/me", {
                    method: "GET",
                    credentials: "include",
                });
                if (!res.ok) throw new Error("Failed to fetch products");
                const data = await res.json();
                setProducts(data);
                console.log(data);
            } catch (error) {
                console.error("Error fetching products:", error);
            }
        };
        fetchProducts();
    }, []);



    return (
        <Table className="productTable">
            <thead>
            <tr>
                <th>#</th>
                <th>Product</th>
                <th>Aantal</th>
                <th>Prijs</th>
                <th>Datum</th>
            </tr>
            </thead>
            <tbody>
            {
                products.map((product: Product, index) => (
                    <tr key={index}>
                        <td>{index + 1}</td>
                        <td>{product.name}</td>
                        <td>{product.quantity}</td>
                        <td>{product.price}</td>
                        <td>{formatDate(product.date)}</td>

                    </tr>
                ))
            }
            </tbody>
        </Table>
    );
}
