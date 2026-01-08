"use client";

import {useState} from "react";
import Button from "react-bootstrap/Button";
import {Table} from "react-bootstrap";
import s from "./history.module.css";
import Image from "next/image";

type HistoryProps = {
    RegisteredProductID: number;
    Name: string;
    Picture: string;
    companyName: string;
}

// Een enkele sale item
interface Sale {
    quantity: number;
    price: number;
    date: string;
}

// Company sale item heeft extra companyName
interface NotCompanySale extends Sale {
    companyName: string;
}

// Hele response types
interface CompanyData {
    avgPrice: number;
    recentSales: Sale[];
}

interface NotCompanyData {
    avgPrice: number;
    recentSales: NotCompanySale[];
}

export default function History(HistoryProps: HistoryProps) {
    const [companyData, setCompanyData] = useState<CompanyData | null>(null);
    const [notCompanyData, setNotCompanyData] = useState<NotCompanyData | null>(null);
    const [loading, setLoading] = useState(false);

    const fetchData = async () => {
        if (loading) return;
        setLoading(true);

        try {
            const resCompany = await fetch(
                `http://localhost:5001/api/v2/AuctionSaleProduct/history/${HistoryProps.RegisteredProductID}/company`,
                {method: "GET", credentials: "include"}
            );
            const company = await resCompany.json();
            setCompanyData(company);

            const resNotCompany = await fetch(
                `http://localhost:5001/api/v2/AuctionSaleProduct/history/${HistoryProps.RegisteredProductID}/not-company`,
                {method: "GET", credentials: "include"}
            );
            const notCompany = await resNotCompany.json();
            setNotCompanyData(notCompany);

            console.log("Fetched company:", company);
            console.log("Fetched not-company:", notCompany);
        } catch (error) {
            console.error("Error fetching history data:", error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <>
            <Button variant="primary" onClick={fetchData} popoverTarget={"history"}>
                {loading ? "Loading..." : "Show History Data"}
            </Button>

            <div id="history" className={s.popover} popover={"auto"}>
                <div className={s.productInfo}>
                    <Image src={HistoryProps.Picture} alt={HistoryProps.Name} width={50} height={50}/>
                    <h3>{HistoryProps.Name}</h3>

                </div>
                <p>Laatste 10 aankopen</p>
                <div className={s.tablesWrapper}>
                    <div>
                        <Table striped className={s.historyTable}>
                            <thead>
                            <tr>
                                <th>Aanbieder: {HistoryProps.Name}</th>
                            </tr>
                            </thead>
                            <tbody>
                            {companyData?.recentSales.map((item, index) => (
                                <tr key={index}>
                                    <td>
                                        <p>
                                            €{item.price} per stuk - ({item.quantity}x voor
                                            €{(item.price * item.quantity).toFixed(2)})
                                            op {new Date(item.date).toLocaleDateString()} <br/>
                                            <span>&nbsp;</span>
                                        </p>

                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </Table>
                        <p>Gemiddelde prijs: €{companyData?.avgPrice.toFixed(2)}</p>
                    </div>
                    <div>
                        <Table striped className={s.historyTable}>
                            <thead>
                            <tr>
                                <th>Andere aanbieders:</th>
                            </tr>
                            </thead>
                            <tbody>
                            {notCompanyData?.recentSales.map((item, index) => (
                                <tr key={index}>
                                    <td>
                                        <p>
                                            ({item.quantity}x) -
                                            €{item.price} op {new Date(item.date).toLocaleDateString()} <br/>
                                            Bedrijf: {item.companyName}
                                        </p>
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </Table>
                        <p>Gemiddelde prijs: €{notCompanyData?.avgPrice.toFixed(2)}</p>
                    </div>
                </div>
            </div>
        </>
    );
}
