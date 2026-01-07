'use client';
import styles from './page.module.css';
import Header from "@/components/header/header";
import DashboardPanel from "@/components/dashboardPanel/dashboardpanel";
import {useState, useEffect} from "react";
import {ClockLocation, parseClockLocation} from "@/enums/ClockLocation";

import { AuctionResult } from "@/types/Auction/AuctionResult";
import {resolveImageSrc} from "@/utils/image";

export default function Home() {
    const [auctions, setAuctions] = useState<AuctionResult[]>([]);
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

                const data: AuctionResult[] = await res.json();

                const live = data.filter((page) => page.auction.isLive);

                // One live auction per clock location
                const uniqueByClock = Object.values(ClockLocation) .filter((v): v is number => typeof v === "number") .map((clockId) => live.find((p) => p.auction.clockLocationEnum === clockId) ) .filter((p): p is AuctionResult => Boolean(p));

                setAuctions(uniqueByClock);
            } catch (err) {
                console.error("Failed to load auctions:", err);
            } finally {
                setLoading(false);
            }
        };

        fetchAuctions();
    }, []);

    useEffect(() => {
        const ids = auctions.map(a => a.auction.id);
        const duplicates = ids.filter((id, idx) => ids.indexOf(id) !== idx);
        if (duplicates.length > 0) console.warn("Duplicate auction ids:", duplicates);
    }, [auctions]);

    return (
        <>
            <Header/>
            <main className={styles.main}>
                <div className={styles.page}>
                    <h1 className={styles.huidigeVeilingen}>Veilingen Dashboard</h1>

                    <div className={styles.panels}>
                        {loading ? (
                            <>
                                {Array.from({ length: 4 }).map((_, i) => (
                                    <DashboardPanel key={i} loading={true} title="Laden..." />
                                ))}
                            </>
                        ) : auctions.length === 0 ? (
                            <p>Geen veilingen gevonden. Kom later terug.</p>
                        ) : (
                            auctions.map((page) => {
                                const auction = page.auction;
                                const reg = page.registeredProducts[0];
                                const product = reg?.product;
                                const nextProduct = page.registeredProducts[1];

                                return (
                                    <a key={`${auction.clockLocationEnum}-${auction.id}`} href={`/veiling/${auction.id}`}>
                                        <DashboardPanel
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={resolveImageSrc(product?.picture)}
                                            resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                            huidigePrijs={reg?.minPrice}
                                            aankomendProductNaam={nextProduct?.product.name || "Geen product"}
                                            aankomendProductStartprijs={nextProduct?.minPrice}
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
