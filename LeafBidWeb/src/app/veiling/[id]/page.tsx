'use client';

import InfoVeld from "@/components/infoVeldKlein/infoVeldKlein";
import BigInfoVeld from "@/components/veilingInfo/veilingInfo";
import Header from "@/components/header/header";
import AuctionTimer from '@/components/veilingKlok/veilingKlok';
import s from "./page.module.css";
import { AuctionPageResult } from "@/types/Auction/AuctionPageResult";
import {getServerNow, getServerOffset, setServerTimeOffset} from "@/utils/time";

import { useParams } from "next/navigation";
import { useCallback, useEffect, useState, useRef } from "react";
import * as signalR from "@microsoft/signalr";

export default function AuctionPage() {
    const params = useParams();
    const id = Number(params.id);

    const [auction, setAuction] = useState<AuctionPageResult | null>(null);
    const [loading, setLoading] = useState(true);

    const [currentPricePerUnit, setCurrentPricePerUnit] = useState<number | null>(null);
    const [now, setNow] = useState(getServerNow().getTime());
    
    useEffect(() => {
        const interval = setInterval(() => setNow(getServerNow().getTime()), 1000);
        return () => clearInterval(interval);
    }, []);

    const startDateTs = auction?.auction?.startDate ? new Date(auction.auction.startDate).getTime() : 0;
    const isPaused = now < startDateTs;
    const pauseCountdown = Math.max(0, Math.ceil((startDateTs - now) / 1000));

    const connectionRef = useRef<signalR.HubConnection | null>(null);

    const onAuctionTimerFinished = useCallback(async () => {
        if (!auction || auction.registeredProducts.length === 0 || isPaused) return;
        const currentProduct = auction.registeredProducts[0];

        console.log("Auction timer finished for product:", currentProduct.id);

        try {
            const res = await fetch(`http://localhost:5001/api/v2/AuctionSaleProduct/expire/${currentProduct.id}?auctionId=${id}`, {
                method: "POST",
                credentials: "include",
            });

            if (!res.ok) {
                console.warn("Expire request failed, will retry...");
                setTimeout(onAuctionTimerFinished, 2000);
            }
        } catch (err) {
            console.error("Error expiring product:", err);
            setTimeout(onAuctionTimerFinished, 2000);
        }
    }, [auction, id, isPaused]);

    const onBuy = useCallback(async (amount: number) => {
        if (!auction || auction.registeredProducts.length === 0 || isPaused) return;
        const currentProduct = auction.registeredProducts[0];

        try {
            const res = await fetch(`http://localhost:5001/api/v2/AuctionSaleProduct/buy`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    registeredProductId: currentProduct.id,
                    auctionId: id,
                    quantity: amount,
                    pricePerUnit: 0, // Ignored by server
                }),
                credentials: "include",
            });

            if (!res.ok) {
                const errorText = await res.text();
                throw new Error(errorText || "Failed to buy product");
            }

            // We don't necessarily need to update state here as SignalR will broadcast it,
            // but for immediate feedback we can.
            // Note: The broadcast will also contain the newStartDate.
        } catch (err) {
            console.error(err);
            alert(err instanceof Error ? err.message : "An error occurred");
        }
    }, [auction, id, isPaused]);

    useEffect(() => {
        if (!id) return;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5001/auctionHub")
            .withAutomaticReconnect()
            .build();

        connection.on("ProductBought", (data: { registeredProductId: number, stock: number, quantityBought: number, newStartDate: string }) => {
            console.log("Real-time update: Product bought", data);
            
            setAuction(prev => {
                if (!prev) return null;
                const newProducts = [...prev.registeredProducts];
                
                if (newProducts.length > 0 && newProducts[0].id === data.registeredProductId) {
                    if (data.stock <= 0) {
                        newProducts.shift();
                    } else {
                        newProducts[0] = { ...newProducts[0], stock: data.stock };
                    }
                }
                
                return { 
                    ...prev, 
                    registeredProducts: newProducts,
                    auction: { ...prev.auction, startDate: data.newStartDate }
                };
            });
        });

        connection.on("ProductExpired", (data: { registeredProductId: number, newStartDate: string }) => {
            console.log("Real-time update: Product expired", data);

            setAuction(prev => {
                if (!prev) return null;
                const newProducts = [...prev.registeredProducts];

                if (newProducts.length > 0 && newProducts[0].id === data.registeredProductId) {
                    newProducts.shift();
                }

                return { 
                    ...prev, 
                    registeredProducts: newProducts,
                    auction: { ...prev.auction, startDate: data.newStartDate }
                };
            });
        });

        connection.start()
            .then(() => {
                console.log("SignalR Connected");
                connection.invoke("JoinAuction", id);
            })
            .catch(err => console.error("SignalR Connection Error: ", err));

        connectionRef.current = connection;

        return () => {
            connection.stop();
        };
    }, [id]);

    useEffect(() => {
        if (!id) return;

        const fetchData = async () => {
            try {
                setLoading(true);

                const res = await fetch(`http://localhost:5001/api/v2/Pages/${id}`, {
                    method: "GET",
                    credentials: "include",
                });

                if (!res.ok) {
                    throw new Error("Failed to fetch auction");
                }

                const data: AuctionPageResult = await res.json();
                
                // Calculate clock offset (Server - Client)
                setServerTimeOffset(data.serverTime);
                setNow(getServerNow().getTime());

                // Filter out products with no stock
                data.registeredProducts = (data.registeredProducts || []).filter(rp => (rp.stock ?? 0) > 0);
                
                setAuction(data);
            } catch (err) {
                console.error(err);
                setAuction(null);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [id]);

    // Keep currentPricePerUnit in sync with the current product's maxPrice
    // This hook must be ABOVE any early returns.
    useEffect(() => {
        const maxPrice = auction?.registeredProducts?.[0]?.maxPrice;

        if (typeof maxPrice === "number" && Number.isFinite(maxPrice)) {
            setCurrentPricePerUnit(maxPrice);
            return;
        }

        setCurrentPricePerUnit(null);
    }, [auction?.registeredProducts]);

    const formatCountdown = (seconds: number) => {
        const m = Math.floor(seconds / 60);
        const s = seconds % 60;
        if (m >= 60) {
            const h = Math.floor(m / 60);
            return `${h}:${(m % 60).toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
        }
        return `${m}:${s.toString().padStart(2, '0')}`;
    };

    if (loading) {
        return (
            <>
                <Header returnOption={true} />
                <main className={s.main}>
                    <h2>Laden...</h2>
                </main>
            </>
        );
    }

    if (!auction || auction.registeredProducts.length === 0) {
        return (
            <>
                <Header returnOption={true} />
                <main className={s.main}>
                    <div className="container mt-5 text-center">
                        <h2>De veiling is gesloten!</h2>
                        {auction ? (
                            <p>Alle producten in deze veiling zijn verkocht of verlopen.</p>
                        ) : (
                            <p>De gevraagde veiling kon niet worden gevonden.</p>
                        )}
                    </div>
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
                <Header returnOption={true} />
                <main className={s.main}>
                    <h2>Prijsinformatie niet beschikbaar</h2>
                </main>
            </>
        );
    }

    return (
        <>
            <Header returnOption={true} />
            <main className={s.main}>
                <div className={s.links}>
                    <div className="App">
                        <AuctionTimer
                            key={currentProduct.id}
                            onFinished={onAuctionTimerFinished}
                            onPriceChange={setCurrentPricePerUnit}
                            maxPrice={maxPrice}
                            minPrice={minPrice}
                            isPaused={isPaused}
                            startDate={auction.auction.startDate}
                            timeOffset={getServerOffset()}
                        />
                        {isPaused && (
                            <div className="alert alert-info mt-2">
                                {!auction.auction.isLive || pauseCountdown > 10
                                    ? `Veiling begint over ${formatCountdown(pauseCountdown)}`
                                    : `Veiling gepauzeerd. Start opnieuw in ${pauseCountdown} seconden...`}
                            </div>
                        )}
                    </div>

                    <h3 className="fw-bold">Volgende Producten:</h3>

                    <div className={s.tekstblokken}>
                        {nextProducts.length > 0 ? (
                            nextProducts.map((rp) => (
                                <InfoVeld key={rp.id} registeredProduct={rp} />
                            ))
                        ) : (
                            <p>Geen volgende producten</p>
                        )}
                    </div>
                </div>

                <div className={s.infoblok}>
                    <h3 className="fw-bold mb-2">Huidig Product:</h3>
                    <BigInfoVeld
                        registeredProduct={currentProduct}
                        currentPricePerUnit={currentPricePerUnit ?? maxPrice}
                        onBuy={onBuy}
                        isPaused={isPaused}
                        isLive={auction.auction.isLive}
                    />
                </div>
            </main>
        </>
    );
}
