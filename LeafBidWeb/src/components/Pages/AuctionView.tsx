'use client';
import styles from './AuctionView.module.css';
import Header from "@/components/Header/Header";
import DashboardPanel from "@/components/DashboardPanel/DashboardPanel";
import {useEffect, useState} from "react";
import {ClockLocation, parseClockLocation} from "@/enums/ClockLocation";

import {AuctionPageResult} from "@/types/Auction/AuctionPageResult";
import {resolveImageSrc} from "@/utils/Image";
import {setServerTimeOffset} from "@/utils/Time";
import {useRouter} from "nextjs-toploader/app";

export default function AuctionView() {
    const router = useRouter();
    const [auctions, setAuctions] = useState<AuctionPageResult[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchAuctions = async (): Promise<void> => {
            setLoading(true);

            try {
                const res = await fetch("http://localhost:5001/api/v2/Pages", {
                    method: "GET",
                    credentials: "include",
                });

                if (!res.ok) {
                    console.error("Failed to fetch auctions");
                    return;
                }

                const data: AuctionPageResult[] = await res.json();

                const visibleOrLive = data.filter(
                    (page) => page.auction.isLive || page.auction.isVisible
                );

                // One live or upcoming auction per clock location
                const uniqueByClock = Object.values(ClockLocation)
                    .filter((v): v is number => typeof v === "number")
                    .map((clockId) => {
                        // Prefer live over just visible
                        return (
                            visibleOrLive.find(
                                (p) =>
                                    p.auction.clockLocationEnum === clockId &&
                                    p.auction.isLive
                            ) ??
                            visibleOrLive.find(
                                (p) =>
                                    p.auction.clockLocationEnum === clockId
                            )
                        );
                    })
                    .filter(
                        (p): p is AuctionPageResult => Boolean(p)
                    );

                setAuctions(uniqueByClock);

                if (data.length > 0) {
                    setServerTimeOffset(data[0].serverTime);
                }
            } catch (err) {
                console.error("Failed to load auctions:", err);
            } finally {
                setLoading(false);
            }
        };

        void fetchAuctions();
    }, []);

    useEffect(() => {
        const ids = auctions.map(a => a.auction.id);
        const duplicates = ids.filter((id, idx) => ids.indexOf(id) !== idx);
        if (duplicates.length > 0) console.warn("Duplicate auction ids:", duplicates);
    }, [auctions]);

    return (
        <>
            <main className={styles.main}>
                <div className={styles.page}>
                    <h1 className={styles.huidigeVeilingen}>Veilingen Dashboard</h1>

                    <div className={styles.panels}>
                        {loading ? (
                            <>
                                {Array.from({length: 4}).map((_, i) => (
                                    <DashboardPanel key={i} loading={true} title="Laden..."/>
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

                                const isLive = auction.isLive;
                                const startTime = new Date(auction.startDate);
                                const auctionStatus = isLive
                                    ? "Live"
                                    : `Start: ${startTime.toLocaleTimeString([], {
                                        hour: '2-digit',
                                        minute: '2-digit'
                                    })}`;

                                return (
                                    <a key={`${auction.clockLocationEnum}-${auction.id}`}
                                       onClick={() => router.push(`/auction/${auction.id}`)} href="#">
                                        <DashboardPanel
                                            loading={false}
                                            title={product?.name ?? `Veiling #${auction.id}`}
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                            imageSrc={resolveImageSrc(product?.picture)}
                                            auctionStatus={auctionStatus}
                                            huidigePrijs={reg?.minPrice}
                                            aankomendProductNaam={nextProduct?.product!.name || "Geen product"}
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
