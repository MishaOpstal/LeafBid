"use client";
import Table from 'react-bootstrap/Table'
import Image from 'next/image';
import {useEffect, useState} from "react";
import {Roles} from "@/enums/Roles";
import History from "@/components/History/History";
import {AuctionSaleProduct} from "@/types/Product/AuctionSaleProduct";
import {resolveImageSrc} from "@/utils/Image";
import config from "@/Config";

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

export default function ProductTable({userRoles}: Props) {
    const [auctionSaleProducts, setAuctionSaleProducts] = useState<AuctionSaleProduct[]>([]);

    useEffect(() => {
        const fetchProducts = async () => {
            try {
                let url = "";

                if (userRoles === Roles.Buyer) {
                    url = `${config.apiUrl}/AuctionSaleProduct/me`;
                } else if (userRoles === Roles.Provider) {
                    url = `${config.apiUrl}/AuctionSaleProduct/company`;
                } else {
                    console.error("Invalid user role");
                    return;
                }

                const res = await fetch(url, {
                    method: "GET",
                    credentials: "include",
                });

                if (!res.ok) {
                    console.error("Fetch failed:", res.status, res.statusText);
                    return;
                }

                const data: AuctionSaleProduct[] = await res.json();
                console.log("Fetched data:", data);

                setAuctionSaleProducts(data);
            } catch (error) {
                console.error("Error fetching products:", error);
            }
        };

        void fetchProducts();
    }, [userRoles]);

    if (userRoles == Roles.Buyer) {
        return (
            <div className="h-100 w-100 overflow-y-scroll rounded-4">
                {auctionSaleProducts.length === 0 ? (
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
                            <th>Historie</th>
                        </tr>
                        </thead>
                        <tbody>
                        {auctionSaleProducts && auctionSaleProducts.map((auctionSaleProduct: AuctionSaleProduct, index) => (
                            <tr key={index}>
                                <td className="align-middle">{index + 1}</td>
                                <td className="align-middle">
                                    <Image
                                        src={resolveImageSrc(auctionSaleProduct.product!.picture!)!}
                                        alt={auctionSaleProduct.product!.name!}
                                        width={40}
                                        height={40}
                                        className="me-2 rounded"
                                    />
                                    <span>{auctionSaleProduct.product!.name!}</span>
                                </td>
                                <td className="align-middle">{auctionSaleProduct.quantity}</td>
                                <td className="align-middle">{auctionSaleProduct.price}</td>
                                <td className="align-middle">{formatDate(auctionSaleProduct.date.toString())}</td>
                                <td className="align-middle">
                                    <History
                                        registeredProductID={auctionSaleProduct.registeredProduct!.id!}
                                        name={auctionSaleProduct.product!.name!}
                                        picture={auctionSaleProduct.product!.picture!}
                                        companyName={auctionSaleProduct.company!.name!}
                                    />
                                </td>
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
                {auctionSaleProducts.length === 0 ? (
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
                        {auctionSaleProducts && auctionSaleProducts.map((auctionSaleProduct: AuctionSaleProduct, index) => (
                            <tr key={index}>
                                <td className="align-middle">{index + 1}</td>
                                <td className="align-middle">
                                    <Image
                                        src={resolveImageSrc(auctionSaleProduct.product!.picture!)!}
                                        alt={auctionSaleProduct.product!.name!}
                                        width={40}
                                        height={40}
                                        className="me-2 rounded"
                                    />
                                    <span>{auctionSaleProduct.product!.name!}</span>
                                </td>
                                <td className="align-middle">{auctionSaleProduct.quantity}</td>
                                <td className="align-middle">{auctionSaleProduct.price}</td>
                                <td className="align-middle">{formatDate(auctionSaleProduct.date.toString())}</td>
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