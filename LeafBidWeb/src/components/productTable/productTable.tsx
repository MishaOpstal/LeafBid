"use client";
import Table from 'react-bootstrap/Table'
import Image from 'next/image';
import {useEffect, useState} from "react";
import {Roles} from "@/enums/Roles";
interface Product {
    picture: string;
    name: string;
    quantity: number;
    price: number;
    date: string;
}
type Props = {
    userRoles: Roles;
};
function formatDate(dateStr: string) {
    const d = new Date(dateStr);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${day}-${month}-${year} | ${hours}:${minutes}`;
}

export default function ProductTable({ userRoles }: Props) {
    const [products, setProducts] = useState([]);
    useEffect(() => {
        const fetchProducts = async () => {
            try {
                const res = await fetch("http://localhost:5001/api/v2/AuctionSaleProduct/me", {
                    method: "GET",
                    credentials: "include",
                });
                const data = await res.json();
                setProducts(data);
            } catch (error) {
                console.error("Error fetching products:", error);
            }
        };
        fetchProducts().then(() => console.log("Fetched products"));
    }, []);

    if (userRoles == Roles.Buyer) {
        return (
            <div className="h-100 w-100 overflow-y-scroll rounded-4">
                {products.length === 0 ? (
                    <div className="d-flex flex-column justify-content-center align-items-center h-100">
                        <h3 className="text-center">Je hebt nog geen producten gekocht.</h3>
                        <p className="text-center">Zodra je een product hebt gekocht, verschijnt het hier.</p>
                    </div>
                ) : (
                    <Table className="productTable" striped>
                        <thead className="sticky-top top-0">
                        <tr>
                            <th>#</th>
                            <th>Product</th>
                            <th>Aantal</th>
                            <th>Prijs</th>
                            <th>Datum</th>
                        </tr>
                        </thead>
                        <tbody>
                        {products.map((product: Product, index) => (
                            <tr key={index}>
                                <td className="align-middle">{index + 1}</td>
                                <td className="align-middle">
                                    <Image
                                        src={product.picture}
                                        alt={product.name}
                                        width={40}
                                        height={40}
                                        className="me-2 rounded"
                                    />
                                    <span>{product.name}</span>
                                </td>
                                <td className="align-middle">{product.quantity}</td>
                                <td className="align-middle">{product.price}</td>
                                <td className="align-middle">{formatDate(product.date)}</td>
                            </tr>

                        ))}
                        </tbody>
                    </Table>
                )}
            </div>
        );
    }
    if (userRoles == Roles.Provider) {
        return (
            <div className="h-100 w-100 overflow-y-scroll rounded-4">
                {products.length === 0 ? (
                    <div className="d-flex flex-column justify-content-center align-items-center h-100">
                        <h3 className="text-center">Je hebt nog geen producten verkocht.</h3>
                        <p className="text-center">Zodra je een product hebt verkocht, verschijnt het hier.</p>
                    </div>
                ) : (
                    <Table className="productTable" striped>
                        <thead className="sticky-top top-0">
                        <tr>
                            <th>#</th>
                            <th>Product</th>
                            <th>Aantal</th>
                            <th>Prijs</th>
                            <th>Datum</th>
                        </tr>
                        </thead>
                        <tbody>
                        {products.map((product: Product, index) => (
                            <tr key={index}>
                                <td className="align-middle">{index + 1}</td>
                                <td className="align-middle">
                                    <Image
                                        src={product.picture}
                                        alt={product.name}
                                        width={40}
                                        height={40}
                                        className="me-2 rounded"
                                    />
                                    <span>{product.name}</span>
                                </td>
                                <td className="align-middle">{product.quantity}</td>
                                <td className="align-middle">{product.price}</td>
                                <td className="align-middle">{formatDate(product.date)}</td>
                            </tr>

                        ))}
                        </tbody>
                    </Table>
                )}
            </div>
        );
    }
    return <div>Geen toegestane rol gevonden.</div>;
}