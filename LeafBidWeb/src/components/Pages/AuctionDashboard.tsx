"use client";

import styles from './AuctionView.module.css';
import DashboardPanel from "@/components/DashboardPanel/DashboardPanel";
import React, {useMemo} from "react";
import {parseClockLocation} from "@/enums/ClockLocation";
import Link from "next/link";
import {getServerNow} from "@/utils/Time";
import Button from "@/components/Input/Button";
import {resolveImageSrc} from "@/utils/Image";
import {useAuctions} from "@/utils/AuctionHelper";
import config from "@/Config";


export default function AuctionDashboard() {
    const {router, auctions, loading} = useAuctions();


    // Send a start request to /api/v2/Auction/start/{id} if the startAuction function is called
    const startAuction = (id: number) => {
        void fetch(`${config.apiUrl}/Auction/start/${id}`, {
            method: "PUT",
            credentials: "include"
        });

        location.reload();
    }

    // Send a stop request to /api/v2/Auction/stop/{id} if the stopAuction function is called
    const stopAuction = (id: number) => {
        void fetch(`${config.apiUrl}/Auction/stop/${id}`, {
            method: "DELETE",
            credentials: "include"
        });

        location.reload();
    }

    const now = getServerNow();

    const currentAuctions = useMemo(
        () => auctions.filter(a => a.auction.isLive),
        [auctions]
    );

    const upcomingAuctions = useMemo(
        () => auctions.filter(a => !a.auction.isLive && new Date(a.auction.startDate) > now),
        [auctions, now]
    );

    const pastAuctions = useMemo(
        () => auctions.filter(a => !a.auction.isLive && new Date(a.auction.startDate) <= now),
        [auctions, now]
    );

    return (
        <>

            <main className={styles.main}>
                <div className={styles.page}>
                    <h1 className={`${styles.huidigeVeilingen} mb-4`}>Veiling dashboard</h1>
                    <Button
                        variant="primary"
                        type="submit"
                        label={"Veiling Toevoegen"}
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
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum!)}
                                            imageSrc={resolveImageSrc(product?.picture)}
                                            auctionStatus={new Date(auction.startDate).toLocaleString()}
                                            isLive={auction.isLive}
                                            onStopAuction={() => stopAuction(auction.id!)}
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
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum!)}
                                            imageSrc={resolveImageSrc(product?.picture)}
                                            auctionStatus={new Date(auction.startDate).toLocaleString()}
                                            isLive={auction.isLive}
                                            isFinished={false}
                                            onStartAuction={() => startAuction(auction.id!)}
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
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum!)}
                                            imageSrc={resolveImageSrc(product?.picture)}
                                            auctionStatus={new Date(auction.startDate).toLocaleString()}
                                            isLive={auction.isLive}
                                            isFinished={true}
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
