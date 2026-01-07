'use client';
import styles from './page.module.css';
import Header from "@/components/header/header";
import DashboardPanel from "@/components/dashboardPanel/dashboardpanel";
import {useState, useEffect} from "react";
import {ClockLocation, parseClockLocation} from "@/enums/ClockLocation";

import { Product } from "@/types/Product/Product";
import { Auction } from "@/types/Auction/Auction";

type RegisteredProduct = {
    id: number;
    product: Product;
    minPrice: number;
    maxPrice: number;
    stock: number;
    region: string;
    harvestedAt: string;
    potSize: number | null;
    stemLength: number;
    providerUserName: string;
};

type AuctionPage = {
    auction: Auction;
    registeredProducts: RegisteredProduct[];
};

export default function Home() {
    const [auctions, setAuctions] = useState<AuctionPage[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchAuctions = async () => {
            setLoading(true);

            try {
                const res = await fetch("http://localhost:5001/api/v2/Pages", {
                    method: "GET",
                    credentials: "include",
                });

                if (!res.ok) return;

                const data: AuctionPage[] = await res.json();

                const live = data.filter((page) => page.auction.isLive);

                // One live auction per clock location
                const uniqueByClock = Object.values(ClockLocation) .filter((v): v is number => typeof v === "number") .map((clockId) => live.find((p) => p.auction.clockLocationEnum === clockId) ) .filter((p): p is AuctionPage => Boolean(p));

                setAuctions(uniqueByClock);
            } catch (err) {
                console.error("Failed to load auctions:", err);
            } finally {
                setLoading(false);
            }
        };

        fetchAuctions();
    }, []);

    return (
        <>
            <Header/>
            <main className={styles.main}>
                <div className={styles.page}>
                    <h1 className={styles.huidigeVeilingen}>Veilingen Dashboard</h1>

                    <div className={styles.panels}>
                        {loading ? (
                            <>
                                <DashboardPanel loading title="Laden..." />
                                <DashboardPanel loading title="Laden..." />
                                <DashboardPanel loading title="Laden..." />
                                <DashboardPanel loading title="Laden..." />
                            </>
                        ) : auctions.length === 0 ? (
                            <p>Geen live veilingen gevonden.</p>
                        ) : (
                            auctions.map((page) => {
                                const auction = page.auction;
                                const reg = page.registeredProducts[0];
                                const product = reg?.product;

                                return (
                                    <a key={auction.id} href={`/veiling/${auction.id}`}>
                                        <DashboardPanel
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={product?.picture}
                                            resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                            huidigePrijs={reg?.minPrice}
                                        />
                                    </a>
                                );
                            })
                        )}
                    </div>
                </div>
            </main>
        </>
    );
}
