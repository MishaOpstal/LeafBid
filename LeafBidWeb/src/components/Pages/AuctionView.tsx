'use client';
import styles from './AuctionView.module.css';
import DashboardPanel from "@/components/DashboardPanel/DashboardPanel";
import {useEffect, useMemo, useState} from "react";
import {parseClockLocation} from "@/enums/ClockLocation";

import {AuctionPageResult} from "@/types/Auction/AuctionPageResult";
import {resolveImageSrc} from "@/utils/Image";
import {getServerNow, setServerTimeOffset} from "@/utils/Time";
import {useRouter} from "nextjs-toploader/app";

export default function AuctionView() {
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
                    if (res.status !== 404) {
                        console.error("Failed to fetch auctions");
                        return;
                    }

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

    const currentAndUpcomingAuctions = useMemo(
        () => {
            const serverTime = getServerNow();

            const eligible = auctions.filter((a) => {
                const start = new Date(a.auction.startDate);

                if (a.auction.isLive) {
                    return true;
                }

                return start > serverTime;
            });

            const byClockLocation = new Map<string | number, (typeof auctions)[number]>();

            for (const auctionItem of eligible) {
                const key = auctionItem.auction!.clockLocationEnum!;
                const existing = byClockLocation.get(key);

                if (!existing) {
                    byClockLocation.set(key, auctionItem);
                    continue;
                }

                const existingIsLive = existing.auction.isLive;
                const candidateIsLive = auctionItem.auction.isLive;

                // Prefer a live auction over a non-live auction
                if (!existingIsLive && candidateIsLive) {
                    byClockLocation.set(key, auctionItem);
                    continue;
                }

                if (existingIsLive && !candidateIsLive) {
                    continue;
                }

                // If both are live or both are non-live, pick the earliest startDate
                const existingTime = new Date(existing.auction.startDate).getTime();
                const candidateTime = new Date(auctionItem.auction.startDate).getTime();

                if (candidateTime < existingTime) {
                    byClockLocation.set(key, auctionItem);
                }
            }

            return Array.from(byClockLocation.values()).sort((a, b) => {
                const aDate = new Date(a.auction.startDate);
                const bDate = new Date(b.auction.startDate);
                return aDate.getTime() - bDate.getTime();
            });
        },
        [auctions]
    );

    useEffect(() => {
        const ids = auctions.map(a => a.auction.id);
        const duplicates = ids.filter((id, idx) => ids.indexOf(id) !== idx);
        if (duplicates.length > 0) console.warn("Duplicate auction ids:", duplicates);
    }, [auctions]);

    return (
        <>
            <main className={styles.main}>
                <div className={styles.page}>
                    <h1 className={styles.huidigeVeilingen}>Huidige Veilingen:</h1>

                    <div className={styles.panels}>
                        {loading ? (
                            <>
                                {Array.from({length: 4}).map((_, i) => (
                                    <DashboardPanel key={i} loading={true} title="Laden..."/>
                                ))}
                            </>
                        ) : currentAndUpcomingAuctions.length === 0 ? (
                            <p>Geen veilingen gevonden. Kom later terug.</p>
                        ) : (
                            currentAndUpcomingAuctions.map((page) => {
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
                                            kloklocatie={parseClockLocation(auction.clockLocationEnum!)}
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
