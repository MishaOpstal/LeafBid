'use client';

import InfoVeld from "@/components/infoVeldKlein/infoVeldKlein";
import BigInfoVeld from "@/components/veilingInfo/veilingInfo";
import Header from "@/components/header/header";
import AuctionTimer from '@/components/veilingKlok/veilingKlok';
import s from "./page.module.css";
import { AuctionResult } from "@/types/Auction/AuctionResult";

import { useParams } from "next/navigation";
import { useEffect, useState } from "react";

export default function AuctionPage() {
    const params = useParams();
    const id = Number(params.id);

    const [auction, setAuction] = useState<AuctionResult | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!id) return;

        const fetchData = async () => {
            try {
                const res = await fetch(`http://localhost:5001/api/v2/Pages/${id}`, {
                    method: "GET",
                    credentials: "include",
                });

                if (!res.ok) {
                    throw new Error("Failed to fetch auction");
                }

                const data: AuctionResult = await res.json();

                setAuction(data);
            } catch (err) {
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [id]);

    if (loading) {
        return (
            <>
                <Header returnOption={true}/>
                <main className={s.main}>
                    <h2>Laden...</h2>
                </main>
            </>
        );
    }

    if (!auction || auction.registeredProducts.length === 0) {
        return (
            <>
                <Header returnOption={true}/>
                <main className={s.main}>
                    <h2>Geen veiling gevonden</h2>
                </main>
            </>
        );
    }

    const currentProduct = auction.registeredProducts[0];
    const nextProducts = auction.registeredProducts.slice(1);

    const maxPrice = currentProduct.maxPrice;
    const minPrice = currentProduct.minPrice;

    if (!maxPrice || !minPrice) {
        return (
            <>
                <Header returnOption={true}/>
                <main className={s.main}>
                    <h2>Prijsinformatie niet beschikbaar</h2>
                </main>
            </>
        );
    }

    return (
        <>
            <Header returnOption={true}/>
            <main className={s.main}>
                <div className={s.links}>
                    <div className="App">

                        <AuctionTimer
                            // duration={90}
                            maxPrice={maxPrice}
                            minPrice={minPrice}
                        />
                    </div>
                    <h3 className="fw-bold">Volgende Producten:</h3>

                    <div className={s.tekstblokken}>
                        {nextProducts.length > 0 ? (
                            nextProducts.map((rp) => <InfoVeld key={rp.id} registeredProduct={rp}/>)
                        ) : (
                            <p>Geen volgende producten</p>
                        )}
                    </div>
                </div>

                <div className={s.infoblok}>
                    <h3 className="fw-bold mb-2">Huidig Product:</h3>
                    <BigInfoVeld registeredProduct={currentProduct}/>
                </div>
            </main>
        </>
    );
}
