// typescript
import React, { useState, useEffect, useRef } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { parsePrice } from '@/types/Product/Product';
import s from './veilingKlok.module.css';

interface AuctionTimerProps {
    onFinished?: () => void;
    maxPrice: number;
    minPrice: number;
}

const AuctionTimer: React.FC<AuctionTimerProps> = ({ onFinished, maxPrice, minPrice }) => {
    const round2 = (v: number) => Math.round(v * 100) / 100;
    const start = round2(Number(maxPrice || 0));
    const min = round2(Number(minPrice || 0));

    // displayed price (0.01 steps)
    const [currentPrice, setCurrentPrice] = useState<number>(start);
    // smooth progress used for the bar (updated each tick)
    const [percentage, setPercentage] = useState<number>(() => (start > min ? 100 : 0));

    const rawRef = useRef<number>(start); // internal precise price (smooth)
    const displayedRef = useRef<number>(start); // last shown price
    const lastTsRef = useRef<number | null>(null);
    const intervalRef = useRef<number | null>(null);

    const formatTime = (totalSeconds: number) => {
        const s = Math.max(0, Math.floor(totalSeconds));
        const m = Math.floor(s / 60);
        const rest = s % 60;
        return `${m}:${rest.toString().padStart(2, '0')}`;
    };

    useEffect(() => {
        // cleanup any previous interval
        if (intervalRef.current !== null) {
            clearInterval(intervalRef.current);
            intervalRef.current = null;
        }
        rawRef.current = start;
        displayedRef.current = start;
        setCurrentPrice(start);
        lastTsRef.current = null;
        setPercentage(start > min ? 100 : 0);

        if (start <= min) {
            setCurrentPrice(round2(min));
            if (onFinished) onFinished();
            return;
        }

        const decreasePerSecond = start * 0.05; // currency units per second
        if (decreasePerSecond <= 0) return;

        const centStep = 0.01;
        const tickMs = 50; // consistent short interval (works whether mouse is over page or not)

        intervalRef.current = window.setInterval(() => {
            const now = performance.now();
            if (lastTsRef.current == null) lastTsRef.current = now;
            const deltaSec = (now - lastTsRef.current) / 1000;
            lastTsRef.current = now;

            // decrease raw price smoothly
            rawRef.current = Math.max(min, rawRef.current - decreasePerSecond * deltaSec);

            // update smooth percentage from raw value
            const rawPct = start > min ? ((rawRef.current - min) / (start - min)) * 100 : 0;
            const clamped = Number.isFinite(rawPct) ? Math.max(0, Math.min(100, rawPct)) : 0;
            setPercentage(clamped);

            // If raw has dropped enough, step the displayed price by integer cents
            const deltaDisplayed = displayedRef.current - rawRef.current;
            const centsToRemove = Math.floor(deltaDisplayed / centStep);
            if (centsToRemove > 0) {
                const newDisplayed = round2(displayedRef.current - centsToRemove * centStep);
                displayedRef.current = newDisplayed;
                // ensure we don't go below min
                if (newDisplayed <= min) {
                    setCurrentPrice(round2(min));
                    setPercentage(0);
                    if (intervalRef.current !== null) {
                        clearInterval(intervalRef.current);
                        intervalRef.current = null;
                    }
                    if (onFinished) onFinished();
                    return;
                } else {
                    setCurrentPrice(newDisplayed);
                }
            }
        }, tickMs);

        return () => {
            if (intervalRef.current !== null) {
                clearInterval(intervalRef.current);
                intervalRef.current = null;
            }
        };
    }, [start, min, onFinished]);

    const remainingSeconds =
        start > min && start * 0.05 > 0 ? (currentPrice - min) / (start * 0.05) : 0;

    return (
        <section className="container mt-4" aria-label={"Veiling klok"}>
            <h2>{parsePrice(Number(currentPrice.toFixed(2)))} </h2>

            <section className={`progress ${s.progress}`} >
                <section
                    className={`progress-bar progress-bar-animated ${s.balkAnimatie}`}
                    role="progressbar"
                    style={{
                        width: `${percentage}%`,
                    }}
                    aria-valuenow={Math.round(percentage * 100) / 100}
                    aria-valuemin={0}
                    aria-valuemax={100}
                />
                <section className={s.balkTekst} aria-hidden>
                    {formatTime(remainingSeconds)}
                </section>
            </section>

            <section className="mt-2">
                {currentPrice <= min && 'De veiling is gesloten!'}
            </section>
        </section>
    );
};

export default AuctionTimer;
