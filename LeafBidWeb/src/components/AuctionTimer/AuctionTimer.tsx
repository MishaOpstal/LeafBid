import React, { useEffect, useMemo, useRef, useState } from "react";
import "bootstrap/dist/css/bootstrap.min.css";
import {parsePrice, RegisteredProduct} from "@/types/Product/RegisteredProducts";
import s from "./AuctionTimer.module.css";

interface AuctionTimerSettings {
    MinDurationForAuctionTimer: number;     // e.g. 30
    UseMaxDurationForAuctionTimer: boolean; // e.g. false
    MaxDurationForAuctionTimer: number;     // e.g. 300
}

interface AuctionTimerProps {
    product: RegisteredProduct;

    isPaused?: boolean;
    startDate?: string | Date;
    timeOffset?: number; // Server time - Client time

    settings: AuctionTimerSettings;

    onFinished?: () => void;
    onPriceChange?: (price: number) => void;

    tickMs?: number; // default: 50
}

function getProductDurationSeconds(
    product: RegisteredProduct,
    settings: AuctionTimerSettings
): number {
    const startPrice: number =
        product.maxPrice !== null && product.maxPrice !== undefined
            ? product.maxPrice
            : product.minPrice;

    const minPrice: number = product.minPrice;
    const rangeCents: number = (startPrice - minPrice) * 100;

    const startCents: number = startPrice * 100;
    const baseDecreaseCentsPerSecond: number = startCents * 0.05;

    if (baseDecreaseCentsPerSecond <= 0) {
        return 0;
    }

    const impliedSeconds: number = rangeCents / baseDecreaseCentsPerSecond;

    let durationSeconds: number = Math.max(
        settings.MinDurationForAuctionTimer,
        impliedSeconds
    );

    if (settings.UseMaxDurationForAuctionTimer) {
        durationSeconds = Math.min(durationSeconds, settings.MaxDurationForAuctionTimer);
    }

    return durationSeconds;
}

const AuctionTimer: React.FC<AuctionTimerProps> = ({
                                                       product,
                                                       isPaused = false,
                                                       startDate,
                                                       timeOffset = 0,
                                                       settings,
                                                       onFinished,
                                                       onPriceChange,
                                                       tickMs = 50,
                                                   }) => {
    const startPrice: number =
        product.maxPrice !== null && product.maxPrice !== undefined
            ? product.maxPrice
            : product.minPrice;

    const startCents: number = Math.round(Number(startPrice || 0) * 100);
    const minCents: number = Math.round(Number(product.minPrice || 0) * 100);

    const [currentCents, setCurrentCents] = useState<number>(startCents);
    const [percentage, setPercentage] = useState<number>(100);

    const rawCentsRef = useRef<number>(startCents);
    const intervalRef = useRef<number | null>(null);

    const onFinishedRef = useRef<(() => void) | undefined>(onFinished);
    const onPriceChangeRef = useRef<((price: number) => void) | undefined>(onPriceChange);

    useEffect(() => {
        onFinishedRef.current = onFinished;
    }, [onFinished]);

    useEffect(() => {
        onPriceChangeRef.current = onPriceChange;
    }, [onPriceChange]);

    const formatTime = (totalSeconds: number) => {
        const seconds: number = Math.max(0, Math.floor(totalSeconds));
        const minutes: number = Math.floor(seconds / 60);
        const rest: number = seconds % 60;
        return `${minutes}:${rest.toString().padStart(2, "0")}`;
    };

    const durationSeconds: number = useMemo(() => {
        return getProductDurationSeconds(product, settings);
    }, [product, settings]);

    const decreaseCentsPerSecond: number = useMemo(() => {
        if (durationSeconds <= 0) {
            return 0;
        }

        const rangeCents: number = Math.max(0, startCents - minCents);
        if (rangeCents <= 0) {
            return 0;
        }

        return rangeCents / durationSeconds;
    }, [durationSeconds, startCents, minCents]);

    // Notify parent of price changes
    useEffect(() => {
        onPriceChangeRef.current?.(currentCents / 100);
    }, [currentCents]);

    useEffect(() => {
        // Clean up previous interval if any
        if (intervalRef.current !== null) {
            clearInterval(intervalRef.current);
            intervalRef.current = null;
        }

        const syncWithTime = () => {
            if (isPaused) {
                return false;
            }

            const now: number = Date.now() + timeOffset;
            const startTs: number = startDate ? new Date(startDate).getTime() : now;
            const deltaSec: number = (now - startTs) / 1000;

            if (deltaSec < 0) {
                rawCentsRef.current = startCents;
                setCurrentCents(startCents);
                setPercentage(100);
                return false;
            }

            const nextRaw: number = startCents - decreaseCentsPerSecond * deltaSec;
            rawCentsRef.current = Math.max(minCents, nextRaw);

            const rawPct: number =
                startCents > minCents
                    ? ((rawCentsRef.current - minCents) / (startCents - minCents)) * 100
                    : 0;

            const clampedPct: number = Number.isFinite(rawPct)
                ? Math.max(0, Math.min(100, rawPct))
                : 0;

            setPercentage(clampedPct);

            const nextDisplayed: number = Math.max(minCents, Math.ceil(rawCentsRef.current));
            setCurrentCents(nextDisplayed);

            if (nextDisplayed <= minCents) {
                setPercentage(0);
                setTimeout(() => {
                    onFinishedRef.current?.();
                }, 0);
                return true;
            }

            return false;
        };

        const isFinished: boolean = syncWithTime();
        if (isFinished || startCents <= minCents || decreaseCentsPerSecond <= 0) {
            return;
        }

        intervalRef.current = window.setInterval(() => {
            const finished: boolean = syncWithTime();
            if (finished && intervalRef.current !== null) {
                clearInterval(intervalRef.current);
                intervalRef.current = null;
            }
        }, tickMs);

        return () => {
            if (intervalRef.current !== null) {
                clearInterval(intervalRef.current);
                intervalRef.current = null;
            }
        };
    }, [startCents, minCents, isPaused, decreaseCentsPerSecond, startDate, timeOffset, tickMs]);

    useEffect(() => {
        // Hard reset whenever a new start moment or new product price range arrives
        rawCentsRef.current = startCents;
        setCurrentCents(startCents);
        setPercentage(100);
    }, [startCents, minCents, startDate]);

    const currentPrice: number = currentCents / 100;

    // Use getProductDurationSeconds
    const secondsSinceStart: number = (Date.now() - new Date(startDate!).getTime()) / 1000;
    const remainingSeconds: number = getProductDurationSeconds(product, settings) - secondsSinceStart;

    return (
        <section className="container mt-4" aria-label="Veiling klok">
            <h2>{parsePrice(Number(currentPrice.toFixed(2)))}</h2>

            <section className={`progress mb-2 ${s.progress}`}>
                <section
                    className={`progress-bar progress-bar-animated ${s.balkAnimatie}`}
                    role="progressbar"
                    style={{ width: `${percentage}%` }}
                    aria-valuenow={Math.round(percentage * 100) / 100}
                    aria-valuemin={0}
                    aria-valuemax={100}
                />
                <section className={s.balkTekst} aria-hidden>
                    {formatTime(remainingSeconds)}
                </section>
            </section>
        </section>
    );
};

export default AuctionTimer;
