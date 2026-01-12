import React, { useEffect, useMemo, useRef, useState } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { parsePrice } from '@/types/Product/RegisteredProducts';
import s from './veilingKlok.module.css';

interface AuctionTimerProps {
    onFinished?: () => void;
    onPriceChange?: (price: number) => void;
    maxPrice: number;
    minPrice: number;
    isPaused?: boolean;
    startDate?: string | Date;
    timeOffset?: number; // Server time - Client time

    // Option B controls
    minDurationSeconds?: number;          // default: 30
    useMaxDuration?: boolean;             // default: false
    maxDurationSeconds?: number;          // default: 300 (only used when useMaxDuration=true)
}

const AuctionTimer: React.FC<AuctionTimerProps> = ({
                                                       onFinished,
                                                       onPriceChange,
                                                       maxPrice,
                                                       minPrice,
                                                       isPaused = false,
                                                       startDate,
                                                       timeOffset = 0,
                                                       minDurationSeconds = 30,
                                                       useMaxDuration = false,
                                                       maxDurationSeconds = 300,
                                                   }) => {
    const startCents: number = Math.round(Number(maxPrice || 0) * 100);
    const minCents: number = Math.round(Number(minPrice || 0) * 100);

    const [currentCents, setCurrentCents] = useState<number>(startCents);
    const [percentage, setPercentage] = useState<number>(100);

    const rawCentsRef = useRef<number>(startCents);
    const lastTsRef = useRef<number | null>(null);
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
        return `${minutes}:${rest.toString().padStart(2, '0')}`;
    };

    const rangeCents: number = Math.max(0, startCents - minCents);

    const decreaseCentsPerSecond: number = useMemo(() => {
        if (rangeCents <= 0) return 0;
        const baseDecreaseCentsPerSecond: number = startCents * 0.05;
        if (baseDecreaseCentsPerSecond <= 0) return 0;
        const impliedSeconds: number = rangeCents / baseDecreaseCentsPerSecond;
        let durationSeconds: number = Math.max(minDurationSeconds, impliedSeconds);
        if (useMaxDuration) {
            durationSeconds = Math.min(durationSeconds, maxDurationSeconds);
        }
        return rangeCents / durationSeconds;
    }, [rangeCents, startCents, minDurationSeconds, useMaxDuration, maxDurationSeconds]);

    // Notify parent of price changes
    useEffect(() => {
        onPriceChangeRef.current?.(currentCents / 100);
    }, [currentCents]);

    useEffect(() => {
        // Cleanup previous interval if any
        if (intervalRef.current !== null) {
            clearInterval(intervalRef.current);
            intervalRef.current = null;
        }

        const syncWithTime = () => {
            const now = Date.now() + timeOffset;
            const startTs = startDate ? new Date(startDate).getTime() : now;
            const deltaSec = (now - startTs) / 1000;

            if (deltaSec < 0) {
                // We are in pause
                rawCentsRef.current = startCents;
                setCurrentCents(startCents);
                setPercentage(100);
                lastTsRef.current = null;
                return false; // Not finished
            }

            const nextRaw = startCents - decreaseCentsPerSecond * deltaSec;
            rawCentsRef.current = Math.max(minCents, nextRaw);

            const rawPct = startCents > minCents
                ? ((rawCentsRef.current - minCents) / (startCents - minCents)) * 100
                : 0;

            const clampedPct = Number.isFinite(rawPct) ? Math.max(0, Math.min(100, rawPct)) : 0;
            setPercentage(clampedPct);

            const nextDisplayed = Math.max(minCents, Math.ceil(rawCentsRef.current));
            setCurrentCents(nextDisplayed);

            if (nextDisplayed <= minCents) {
                setPercentage(0);
                setTimeout(() => onFinishedRef.current?.(), 0);
                return true; // Finished
            }
            
            lastTsRef.current = now;
            return false;
        };

        // Initial sync
        const isFinished = syncWithTime();
        if (isFinished || startCents <= minCents || decreaseCentsPerSecond <= 0) {
            return;
        }

        const tickMs: number = 50;
        intervalRef.current = window.setInterval(() => {
            const finished = syncWithTime();
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
    }, [startCents, minCents, isPaused, decreaseCentsPerSecond, startDate, timeOffset]);

    useEffect(() => {
        // Hard reset whenever a new start moment or new product price range arrives
        rawCentsRef.current = startCents;
        lastTsRef.current = null;
        setCurrentCents(startCents);
        setPercentage(100);
    }, [startCents, minCents, startDate]);

    const currentPrice: number = currentCents / 100;

    const remainingSeconds: number =
        decreaseCentsPerSecond > 0 ? (currentCents - minCents) / decreaseCentsPerSecond : 0;

    return (
        <section className="container mt-4" aria-label="Veiling klok">
            <h2>{parsePrice(Number(currentPrice.toFixed(2)))} </h2>

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
