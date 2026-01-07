'use client';
import styles from './page.module.css';
import Header from "@/components/header/header";
import DashboardPanel from "@/components/dashboardPanel/dashboardpanel";
import {useState, useEffect} from "react";
import {Auction} from "@/types/Auction/Auction";
import {ClockLocation, parseClockLocation} from "@/enums/ClockLocation";

export default function Home() {
    const [auctions, setAuctions] = useState<Auction[]>([]);
    const [loading, setLoading] = useState(true);

    const clockIds = Object.values(ClockLocation).filter(
        (v): v is number => typeof v === "number"
    );

    useEffect(() => {
        const fetchAuctions = async () => {
            setLoading(true);

            try {
                // Fetch all auctions in parallel
                const results = await Promise.all(
                    clockIds.map(async (clockLocation) => {
                        const res = await fetch(`http://localhost:5001/api/v2/Pages/closest/${clockLocation}`, {
                            method: "GET",
                            credentials: "include",
                        });
                        if (!res.ok) return null;

                        const data = await res.json();

                        const auction: Auction = {
                            ...data.auction,
                            products: data.products,
                        };

                        return auction;
                    })
                );

                // Filter out null responses (failed fetches)
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

    return (
        <>
            <Header/>
            <main className={styles.main}>
                <div className={styles.page}>
                    <h1 className={styles.huidigeVeilingen}>Veilingen Dashboard</h1>

                    <div className={styles.panels}>
                        {loading ? (
                            <>
                                <DashboardPanel loading={true} title="Laden..."/>
                                <DashboardPanel loading={true} title="Laden..."/>
                                <DashboardPanel loading={true} title="Laden..."/>
                                <DashboardPanel loading={true} title="Laden..."/></>
                        ) : auctions.length === 0 ? (
                            <p>Geen veilingen gevonden. Kom later terug.</p>
                        ) : (
                            auctions.map((auction) => {
                                const product = auction.products?.[0];
                                const nextProduct = auction.products?.[1];

                                return (
                                    <>
                                        <a key={`auction-${auction.id}`} href={`/veiling/${auction.id}`}>
                                            <DashboardPanel
                                                key={auction.id}
                                                loading={false}
                                                title={product ? product.name : `Auction #${auction.id}`}
                                                kloklocatie={parseClockLocation(auction.clockLocationEnum)}
                                                imageSrc={product?.picture ? `http://localhost:5001/uploads/${product.picture}` : undefined}
                                                resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                                huidigePrijs={product?.minPrice}
                                                aankomendProductNaam={nextProduct?.name || "Geen product"}
                                                aankomendProductStartprijs={nextProduct?.minPrice}
                                            />
                                        </a>
                                    </>
                                );
                            })
                        )}
                    </div>
                </div>
            </main>
        </>
    );
}
