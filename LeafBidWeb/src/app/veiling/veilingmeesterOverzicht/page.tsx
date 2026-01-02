'use client';

import styles from "@/app/page.module.css";
import Header from "@/components/header/header";
import DashboardPanel from "@/components/dashboardPanel/dashboardpanel";
import { useState, useEffect } from "react";
import { Auction } from "@/types/Auction/Auction";
import { ClockLocation, parseClockLocation } from "@/enums/ClockLocation";
import Link from "next/link";

export default function VeilingmeesterOverzicht() {
    const [auctions, setAuctions] = useState<Auction[]>([]);
    const [loading, setLoading] = useState(true);

    // Extract numeric enum values (1,2,3...)
    const clockIds = Object.values(ClockLocation).filter(
        (v): v is number => typeof v === "number"
    );

    useEffect(() => {
        const fetchAuctions = async () => {
            setLoading(true);

            try {
                const results = await Promise.all(
                    clockIds.map(async (clockLocation) => {
                        const res = await fetch(
                            `http://localhost:5001/api/v2/Pages/closest/${clockLocation}`,
                            { method: "GET", credentials: "include" }
                        );

                        if (!res.ok) return null;

                        const data = await res.json();

                        const auction: Auction = {
                            ...data.auction,
                            products: data.products ?? []
                        };

                        return auction;
                    })
                );

                const filtered = results.filter((a): a is Auction => a !== null);
                setAuctions(filtered);
            } catch (err) {
                console.error("Failed to load auctions:", err);
            } finally {
                setLoading(false);
            }
        };

        fetchAuctions();
    }, []);

    // Categorize auctions
    const now = new Date();

    const currentAuctions = auctions.filter(a => new Date(a.startDate) <= now);
    const upcomingAuctions = auctions.filter(a => new Date(a.startDate) > now);

    // No endDate yet → empty
    const pastAuctions: Auction[] = [];

    return (
        <>
            <Header />

            <main className={styles.main}>
                <div className={styles.page}>
                    <h1 className={styles.huidigeVeilingen}>Veilingmeester Dashboard</h1>

                    {/* ---------------------- HUIDIGE VEILINGEN ---------------------- */}
                    <h2 className={styles.sectionTitle}>Huidige veilingen</h2>
                    <div className={styles.panels}>
                        {loading ? (
                            <>
                                <DashboardPanel compact loading title="Laden..." />
                                <DashboardPanel compact loading title="Laden..." />
                                <DashboardPanel compact loading title="Laden..." />
                            </>
                        ) : currentAuctions.length === 0 ? (
                            <DashboardPanel compact loading title="Geen veilingen beschikbaar..." />
                        ) : (
                            currentAuctions.map((auction) => {
                                const product = auction.products?.[0] ?? null;

                                return (
                                    <Link key={auction.id} href={`/veiling/${auction.id}`}>
                                        <DashboardPanel
                                            compact
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={
                                                product?.picture
                                                    ? `http://localhost:5001/uploads/${product.picture}`
                                                    : undefined
                                            }
                                            resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                        />
                                    </Link>
                                );
                            })
                        )}
                    </div>

                    {/* ---------------------- AANKOMENDE VEILINGEN ---------------------- */}
                    <h2 className={styles.sectionTitle}>Aankomende veilingen</h2>
                    <div className={styles.panels}>
                        {loading ? (
                            <>
                                <DashboardPanel compact loading title="Laden..." />
                                <DashboardPanel compact loading title="Laden..." />
                            </>
                        ) : upcomingAuctions.length === 0 ? (
                            <DashboardPanel compact loading title="Geen veilingen beschikbaar..." />
                        ) : (
                            upcomingAuctions.map((auction) => {
                                const product = auction.products?.[0] ?? null;

                                return (
                                    <Link key={auction.id} href={`/veiling/${auction.id}`}>
                                        <DashboardPanel
                                            compact
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={
                                                product?.picture
                                                    ? `http://localhost:5001/uploads/${product.picture}`
                                                    : undefined
                                            }
                                            resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                        />
                                    </Link>
                                );
                            })
                        )}
                    </div>

                    {/* ---------------------- AFGELOPEN VEILINGEN ---------------------- */}
                    <h2 className={styles.sectionTitle}>Afgelopen veilingen</h2>
                    <div className={styles.panels}>
                        {loading ? (
                            <DashboardPanel compact loading title="Laden..." />
                        ) : pastAuctions.length === 0 ? (
                            <DashboardPanel compact loading title="Geen veilingen beschikbaar..." />
                        ) : (
                            pastAuctions.map((auction) => {
                                const product = auction.products?.[0] ?? null;

                                return (
                                    <Link key={auction.id} href={`/veiling/${auction.id}`}>
                                        <DashboardPanel
                                            compact
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={
                                                product?.picture
                                                    ? `http://localhost:5001/uploads/${product.picture}`
                                                    : undefined
                                            }
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
