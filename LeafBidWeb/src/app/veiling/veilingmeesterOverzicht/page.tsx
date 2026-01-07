'use client';

import styles from "@/app/page.module.css";
import Header from "@/components/header/header";
import DashboardPanel from "@/components/dashboardPanel/dashboardpanel";
import { useState, useEffect } from "react";
import { ClockLocation, parseClockLocation } from "@/enums/ClockLocation";
import Link from "next/link";

import { AuctionPage } from "@/types/Auction/AuctionPage";


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

    const activeClockLocations = auctions
        .filter(a => a.auction.isLive)
        .map(a => parseClockLocation(a.auction.clockLocationEnum));




    const currentAuctions = auctions.filter(a => a.auction.isLive);

    const upcomingAuctions = auctions.filter(
        a => !a.auction.isLive && new Date(a.auction.startDate) > now
    );

    const pastAuctions = auctions.filter(
        a => !a.auction.isLive && new Date(a.auction.startDate) <= now
    );

    async function startAuction(id: number | undefined) {
        if (!id) return;

        await fetch(`http://localhost:5001/api/v2/auction/isLive?auctionId=${id}&isLive=true`, {
            method: "PUT",
            credentials: "include"
        });

        window.location.reload();
    }



    async function stopAuction(id: number | undefined) {
        if (!id) return;

        await fetch(`http://localhost:5001/api/v2/auction/isLive?auctionId=${id}&isLive=false`, {
            method: "PUT",
            credentials: "include"
        });

        window.location.reload();
    }



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

                                            isLive={auction.isLive}
                                            activeClockLocations={activeClockLocations}

                                            onStartAuction={() => startAuction(auction.id)}
                                            onStopAuction={() => stopAuction(auction.id)}

                                            onError={(msg) => alert(msg)}
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

                                            isLive={auction.isLive}
                                            activeClockLocations={activeClockLocations}

                                            onStartAuction={() => startAuction(auction.id)}
                                            onStopAuction={() => stopAuction(auction.id)}

                                            onError={(msg) => alert(msg)}
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

                                            isLive={auction.isLive}
                                            activeClockLocations={activeClockLocations}

                                            onStartAuction={() => startAuction(auction.id)}
                                            onStopAuction={() => stopAuction(auction.id)}

                                            onError={(msg) => alert(msg)}
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
