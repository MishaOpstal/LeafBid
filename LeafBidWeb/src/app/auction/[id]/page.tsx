'use client';

import UpcomingProduct from "@/components/UpcomingProduct/UpcomingProduct";
import AuctionedProduct from "@/components/AuctionedProduct/AuctionedProduct";
import Header from "@/components/Header/Header";
import AuctionTimer from '@/components/AuctionTimer/AuctionTimer';
import s from "./page.module.css";
import {AuctionPageResult} from "@/types/Auction/AuctionPageResult";
import {getServerNow, getServerOffset, setServerTimeOffset} from "@/utils/Time";
import config from "@/Config";
import History from "@/components/History/History";

import {useParams} from "next/navigation";
import {useCallback, useEffect, useRef, useState} from "react";
import * as signalR from "@microsoft/signalr";
import {Toast, ToastContainer} from "react-bootstrap";

export default function AuctionPage() {
    const params = useParams();
    const id = Number(params.id);

    const [auction, setAuction] = useState<AuctionPageResult | null>(null);
    const [loading, setLoading] = useState(true);
    const [toasts, setToasts] = useState<Array<{
        toastId: string;
        id: number;
        name: string;
        picture: string;
        companyName: string
    }>>([]);


    const [currentPricePerUnit, setCurrentPricePerUnit] = useState<number | null>(null);
    const [now, setNow] = useState(() => getServerNow());

    useEffect(() => {
        const interval = setInterval(() => {
            setNow(getServerNow());
        }, 100); // of 250ms
        return () => clearInterval(interval);
    }, []);

    const startDateTs = auction?.auction?.startDate ? new Date(auction.auction.startDate).getTime() : 0;
    const timeToNextProductTs = auction?.auction?.nextProductStartTime ? new Date(auction.auction.nextProductStartTime).getTime() : 0;
    const startCountdown = Math.max(0, Math.ceil((startDateTs - now.getTime()) / 1000));
    const pauseCountdown = Math.max(0, Math.ceil((timeToNextProductTs - now.getTime()) / 1000));
    const isPaused = pauseCountdown > 0;

    const connectionRef = useRef<signalR.HubConnection | null>(null);

    const onAuctionTimerFinished = useCallback(async () => {
        if (!auction || auction.registeredProducts.length === 0 || isPaused) {
            return;
        }

        const currentProduct = auction.registeredProducts[0];

        console.log("Auction timer finished for product:", currentProduct.id);
    }, [auction, isPaused]);

    const onBuy = useCallback(async (amount: number) => {
        if (!auction || auction.registeredProducts.length === 0 || isPaused) {
            return;
        }

        const currentProduct = auction.registeredProducts[0];

        try {
            const res = await fetch(`${config.apiUrl}/Product/buy`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    registeredProductId: currentProduct.id,
                    auctionId: id,
                    quantity: amount,
                }),
                credentials: "include",
            });

            if (!res.ok) {
                const errorText = await res.text();
                console.error("Failed to buy product:", errorText);
                alert(errorText);
                return;
            }

            const product = currentProduct.product;
            if (!product) {
                return;
            }

            setToasts(prev => [
                ...prev,
                {
                    toastId: crypto.randomUUID(), // << uniek
                    id: currentProduct.id,
                    name: product.name,
                    picture: product.picture ?? "",
                    companyName: currentProduct.company?.name ?? ""
                }
            ]);


            // We don't necessarily need to update the state here as SignalR will broadcast it,
            // but for immediate feedback we can.
            // Note: The broadcast will also contain the new nextProductStartTime.
        } catch (err) {
            console.error(err);
            alert(err instanceof Error ? err.message : "An error occurred");
        }
    }, [auction, id, isPaused]);

    useEffect(() => {
        if (!id) return;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(config.hubUrl)
            .withAutomaticReconnect()
            .build();

        connection.on("AuctionStarted", () => {
            // console.log("Real-time update: Auction started", data);
            setAuction(prev => {
                if (!prev) {
                    return null;
                }

                return {
                    ...prev,
                    auction: {...prev.auction, isLive: true}
                };
            });

            location.reload();
        });

        connection.on("AuctionStopped", () => {
            // console.log("Real-time update: Auction stopped", data);
            setAuction(prev => {
                if (!prev) return null;
                return {
                    ...prev,
                    auction: {...prev.auction, isLive: false}
                };
            });
        });

        connection.on("ProductBought", (data: {
            registeredProductId: number,
            stock: number,
            quantityBought: number,
            nextProductStartTime: string
        }) => {
            try {
                // console.log("Real-time update: Product bought", data);

                setAuction(prev => {
                    if (!prev) return null;
                    const newProducts = [...prev.registeredProducts];

                    if (newProducts.length > 0 && newProducts[0].id === data.registeredProductId) {
                        if (data.stock <= 0) {
                            newProducts.shift();
                        } else {
                            newProducts[0] = {...newProducts[0], stock: data.stock};
                        }
                    }

                    return {
                        ...prev,
                        registeredProducts: newProducts,
                        auction: {...prev.auction, nextProductStartTime: data.nextProductStartTime}
                    };
                });
            } catch (error) {
                console.error("Error processing ProductBought event:", error);
            }
        });

        connection.on("ProductExpired", (data: { registeredProductId: number, nextProductStartTime: string }) => {
            try {
                // console.log("Real-time update: Product expired", data);

                setAuction(prev => {
                    if (!prev) return null;
                    const newProducts = [...prev.registeredProducts];

                    if (newProducts.length > 0 && newProducts[0].id === data.registeredProductId) {
                        newProducts.shift();
                    }

                    return {
                        ...prev,
                        registeredProducts: newProducts,
                        auction: {...prev.auction, nextProductStartTime: data.nextProductStartTime}
                    };
                });
            } catch (error) {
                console.error("Error processing ProductExpired event:", error);
            }
        });

        connection.start()
            .then(() => {
                // console.log("SignalR Connected");
                void connection.invoke("JoinAuction", id);
            })
            .catch(err => console.error("SignalR Connection Error: ", err));

        connectionRef.current = connection;
    }, [id]);

    useEffect(() => {
        const fetchData = async (): Promise<void> => {
            try {
                setLoading(true);

                const res = await fetch(`${config.apiUrl}/Pages/${id}`, {
                    method: "GET",
                    credentials: "include",
                });

                if (!res.ok) {
                    console.error("Failed to fetch auction");
                    return;
                }

                const data: AuctionPageResult = await res.json();

                // Calculate clock offset (Server - Client)
                setServerTimeOffset(data.serverTime);
                setNow(getServerNow());

                // Filter out products with no stock
                data.registeredProducts = (data.registeredProducts || []).filter(
                    (rp) => (rp.stock ?? 0) > 0
                );

                setAuction(data);
            } catch (err) {
                console.error("Failed to load auction:", err);
                setAuction(null);
            } finally {
                setLoading(false);
            }
        };

        void fetchData();
    }, [id]);

    // Keep the currentPricePerUnit in sync with the current product's maxPrice
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
                <Header returnOption={true}/>
                <main className={s.main}>
                    <h2>Prijsinformatie niet beschikbaar</h2>
                </main>
            </>
        );
    }

    const isLive = auction.auction.isLive;
    const shouldDisplayMessage = !isLive || isPaused;

    const messages = [
        {cond: () => !isLive, msg: () => `Veiling begint over ${formatCountdown(startCountdown)}`},
        {cond: () => isPaused, msg: () => `Veiling gepauzeerd. Start opnieuw in ${pauseCountdown} seconden...`}
    ];

    const countdownMessage =
        messages.find(({cond}) => cond())?.msg() ?? "";

    function closeToast(toastId: string) {
        setToasts(prev => prev.filter(t => t.toastId !== toastId));
    }


    return (
        <>
            <Header returnOption={true}/>
            <main className={s.main}>
                <div className={s.left}>
                    <div className="App">
                        <AuctionTimer
                            product={currentProduct}
                            settings={{
                                MinDurationForAuctionTimer: 30,
                                UseMaxDurationForAuctionTimer: false,
                                MaxDurationForAuctionTimer: 300,
                            }}
                            isPaused={isPaused}
                            startDate={auction.auction.nextProductStartTime}
                            timeOffset={getServerOffset()}

                            onFinished={onAuctionTimerFinished}
                            onPriceChange={setCurrentPricePerUnit}
                        />

                        {shouldDisplayMessage && (
                            <div className="alert alert-info mt-2">
                                {countdownMessage}
                            </div>
                        )}
                    </div>

                    <h3 className="fw-bold">Volgende Producten:</h3>

                    <div className={s.textBlocks}>
                        {nextProducts.length > 0 ? (
                            nextProducts.map((rp) => (
                                <UpcomingProduct key={rp.id} registeredProduct={rp}/>
                            ))
                        ) : (
                            <p>Geen volgende producten</p>
                        )}
                    </div>
                </div>

                <div className={s.infoBlock}>
                    <h3 className="fw-bold mb-2">Huidig Product:</h3>
                    <AuctionedProduct
                        registeredProduct={currentProduct}
                        currentPricePerUnit={currentPricePerUnit ?? maxPrice}
                        onBuy={onBuy}
                        isPaused={isPaused}
                        isLive={auction.auction.isLive}
                    />
                </div>
            </main>

            <ToastContainer position="bottom-end">
                {toasts.map((item) => (
                    <Toast
                        key={item.toastId}
                        onClose={() => closeToast(item.toastId)}
                        show={true}
                    >

                        <Toast.Header closeButton>
                            <strong className="me-auto">{item.name} gekocht!</strong>
                        </Toast.Header>

                        <Toast.Body>
                            Bekijk de prijshistorie hier:{" "}
                            <History
                                registeredProductID={item.id}
                                name={item.name}
                                picture={item.picture}
                                companyName={item.companyName}
                            />
                        </Toast.Body>
                    </Toast>
                ))}
            </ToastContainer>

        </>
    );
}