'use client';

import styles from "@/app/auction/view/page.module.css";
import Header from "@/components/Header/Header";
import DashboardPanel from "@/components/DashboardPanel/DashboardPanel";
import React, {useEffect, useState} from "react";
import {parseClockLocation} from "@/enums/ClockLocation";
import Link from "next/link";
import {AuctionPageResult} from "@/types/Auction/AuctionPageResult";
import {getServerNow, setServerTimeOffset} from "@/utils/Time";
import Button from "@/components/Input/Button";
import {useRouter} from "nextjs-toploader/app";


export default function AuctionDashboard() {
    const router = useRouter();
    const [auctions, setAuctions] = useState<AuctionPageResult[]>([]);
    const [loading, setLoading] = useState(true);

    // Fetch auctions from the server
    useEffect(() => {
        const loadAuctions = async (): Promise<void> => {
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

                const data: AuctionPageResult[] = await res.json();
                setAuctions(data);

                if (data.length > 0) {
                    setServerTimeOffset(data[0].serverTime);
                }
            } catch (err) {
                console.error("Failed to load auctions:", err);
            } finally {
                setLoading(false);
            }
        };

        void loadAuctions();
    }, []);


    const now = getServerNow();

    const currentAuctions = auctions.filter(a => a.auction.isLive);

    const upcomingAuctions = auctions.filter(
        a => a.auction.isVisible && !a.auction.isLive && new Date(a.auction.startDate) > now
    );

    const pastAuctions = auctions.filter(
        a => !a.auction.isLive && new Date(a.auction.startDate) <= now
    );

    return (
        <>

            <main className={styles.main}>
                <div className={styles.page}>
                    <h1 className={`${styles.huidigeVeilingen} mb-4`}>Veilingmeester Dashboard</h1>
                    <Button
                        variant="primary"
                        type="submit"
                        label={"Veiling Aanmaken"}
                        onClick={() => router.push("/auction/create")}
                    />
                    <div className={`${styles.panels} mt-4`}>
                        <h2 className={styles.sectionTitle}>Huidige veilingen</h2>
                        {loading ? (
                            <>
                                <DashboardPanel compact loading title="Laden..."/>
                                <DashboardPanel compact loading title="Laden..."/>
                                <DashboardPanel compact loading title="Laden..."/>
                            </>
                        ) : currentAuctions.length === 0 ? (
                            <p>Geen huidige veilingen beschikbaar</p>
                        ) : (
                            currentAuctions.map((page) => {
                                const auction = page.auction;
                                const reg = page.registeredProducts[0];
                                const product = reg?.product;

                                return (
                                    <Link key={auction.id} href={`/auction/${auction.id}`}>
                                        <DashboardPanel
                                            compact
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={product?.picture}
                                            auctionStatus={new Date(auction.startDate).toLocaleString()}
                                        />
                                    </Link>
                                );
                            })
                        )}
                    </div>

                    <div className={`${styles.panels} mt-4`}>
                        <h2 className={styles.sectionTitle}>Aankomende veilingen</h2>
                        {loading ? (
                            <>
                                <DashboardPanel compact loading title="Laden..."/>
                                <DashboardPanel compact loading title="Laden..."/>
                            </>
                        ) : upcomingAuctions.length === 0 ? (
                            <p>Geen aankomende veilingen beschikbaar</p>
                        ) : (
                            upcomingAuctions.map((page) => {
                                const auction = page.auction;
                                const reg = page.registeredProducts[0];
                                const product = reg?.product;

                                return (
                                    <Link key={auction.id} href={`/auction/${auction.id}`}>
                                        <DashboardPanel
                                            compact
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={product?.picture}
                                            auctionStatus={new Date(auction.startDate).toLocaleString()}
                                        />
                                    </Link>
                                );
                            })
                        )}
                    </div>

                    <div className={`${styles.panels} mt-4`}>
                        <h2 className={styles.sectionTitle}>Afgelopen veilingen</h2>
                        {loading ? (
                            <DashboardPanel compact loading title="Laden..."/>
                        ) : pastAuctions.length === 0 ? (
                            <p>Geen afgelopen veilingen beschikbaar</p>
                        ) : (
                            pastAuctions.map((page) => {
                                const auction = page.auction;
                                const reg = page.registeredProducts[0];
                                const product = reg?.product;

                                return (
                                    <Link key={auction.id} href={`/auction/${auction.id}`}>
                                        <DashboardPanel
                                            compact
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={product?.picture}
                                            auctionStatus={new Date(auction.startDate).toLocaleString()}
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
