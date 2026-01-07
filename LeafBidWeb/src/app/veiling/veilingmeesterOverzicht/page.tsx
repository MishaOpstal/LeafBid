'use client';

import styles from "@/app/page.module.css";
import Header from "@/components/header/header";
import DashboardPanel from "@/components/dashboardPanel/dashboardpanel";
import { useState, useEffect } from "react";
import { ClockLocation, parseClockLocation } from "@/enums/ClockLocation";
import Link from "next/link";

import { Auction } from "@/types/Auction/Auction";
import { Product } from "@/types/Product/Product";

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


export default function VeilingmeesterOverzicht() {
    const [auctions, setAuctions] = useState<AuctionPage[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchAuctions = async () => {
            setLoading(true);

            try {
                const res = await fetch("http://localhost:5001/api/v2/Pages", {
                    method: "GET",
                    credentials: "include"
                });

                if (!res.ok) {
                    console.error("Failed to fetch auctions");
                    return;
                }

                const data: AuctionPage[] = await res.json();
                setAuctions(data);
            } catch (err) {
                console.error("Failed to load auctions:", err);
            } finally {
                setLoading(false);
            }
        };

        fetchAuctions();
    }, []);


    const now = new Date();

    const currentAuctions = auctions.filter(a => a.auction.isLive);

    const upcomingAuctions = auctions.filter(
        a => !a.auction.isLive && new Date(a.auction.startDate) > now
    );

    const pastAuctions = auctions.filter(
        a => !a.auction.isLive && new Date(a.auction.startDate) <= now
    );


    return (
        <>
            <Header returnOption={true} />

            <main className={styles.main}>
                <div className={styles.page}>
                    <h1 className={styles.huidigeVeilingen}>Veilingmeester Dashboard</h1>

                    <h2 className={styles.sectionTitle}>Huidige veilingen</h2>
                    <div className={styles.panels}>
                        {loading ? (
                            <>
                                <DashboardPanel compact loading title="Laden..." />
                                <DashboardPanel compact loading title="Laden..." />
                                <DashboardPanel compact loading title="Laden..." />
                            </>
                        ) : currentAuctions.length === 0 ? (
                            <p>Geen huidige veilingen beschikbaar</p>
                        ) : (
                            currentAuctions.map((page) => {
                                const auction = page.auction;
                                const reg = page.registeredProducts[0];
                                const product = reg?.product;

                                return (
                                    <Link key={auction.id} href={`/veiling/${auction.id}`}>
                                        <DashboardPanel
                                            compact
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={product?.picture}
                                            resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                        />
                                    </Link>
                                );
                            })
                        )}
                    </div>

                    <h2 className={styles.sectionTitle}>Aankomende veilingen</h2>
                    <div className={styles.panels}>
                        {loading ? (
                            <>
                                <DashboardPanel compact loading title="Laden..." />
                                <DashboardPanel compact loading title="Laden..." />
                            </>
                        ) : upcomingAuctions.length === 0 ? (
                            <p>Geen aankomende veilingen beschikbaar</p>
                        ) : (
                            upcomingAuctions.map((page) => {
                                const auction = page.auction;
                                const reg = page.registeredProducts[0];
                                const product = reg?.product;

                                return (
                                    <Link key={auction.id} href={`/veiling/${auction.id}`}>
                                        <DashboardPanel
                                            compact
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={product?.picture}
                                            resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                        />
                                    </Link>
                                );
                            })
                        )}
                    </div>

                    <h2 className={styles.sectionTitle}>Afgelopen veilingen</h2>
                    <div className={styles.panels}>
                        {loading ? (
                            <DashboardPanel compact loading title="Laden..." />
                        ) : pastAuctions.length === 0 ? (
                            <p>Geen afgelopen veilingen beschikbaar</p>
                        ) : (
                            pastAuctions.map((page) => {
                                const auction = page.auction;
                                const reg = page.registeredProducts[0];
                                const product = reg?.product;

                                return (
                                    <Link key={auction.id} href={`/veiling/${auction.id}`}>
                                        <DashboardPanel
                                            compact
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={product?.picture}
                                            resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                        />
                                    </Link>
                                );
                            })
                        )}
                    </div>
                </div>
            </main>
        </>
    );
}
