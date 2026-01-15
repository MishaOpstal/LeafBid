"use client";

import {useEffect, useMemo, useState} from "react";
import {Pie, PieChart, ResponsiveContainer, Tooltip} from "recharts";
import config from "@/Config";

interface SaleChartDataPoint {
    productName: string;
    price: number;
    quantity: number;

    [key: string]: string | number;
}

interface SaleChartResponse {
    currentMonthData: SaleChartDataPoint[];
    allTimeData: SaleChartDataPoint[];
}

export default function Chart() {
    const [chartData, setChartData] = useState<SaleChartResponse | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let isMounted: boolean = true;

        const fetchChartData = async (): Promise<void> => {
            try {
                setIsLoading(true);
                setError(null);

                const res = await fetch(`${config.apiUrl}/AuctionSaleProduct/chart`, {
                    method: "GET",
                    credentials: "include",
                    cache: "no-store",
                });

                if (!res.ok) {
                    console.error("Failed to fetch chart data:", res.text());
                    return;
                }

                const data = (await res.json()) as SaleChartResponse;

                if (isMounted) {
                    setChartData(data);
                }
            } catch (e) {
                if (isMounted) {
                    const message: string = e instanceof Error ? e.message : "Unknown error";
                    setError(message);
                    setChartData(null);
                }
            } finally {
                if (isMounted) {
                    setIsLoading(false);
                }
            }
        };

        void fetchChartData();

        return () => {
            isMounted = false;
        };
    }, []);

    const currentMonthData: SaleChartDataPoint[] = useMemo(
        () => chartData?.currentMonthData ?? [],
        [chartData?.currentMonthData]
    );

    const allTimeData: SaleChartDataPoint[] = useMemo(
        () => chartData?.allTimeData ?? [],
        [chartData?.allTimeData]
    );

    const currentMonthTotal: string = useMemo(() => {
        // Total is the sum of all sale pricePerUnit times the quantity
        const total: number = currentMonthData.reduce((acc, item) => acc + Number(item.price || 0) * Number(item.quantity || 0), 0);
        return total.toFixed(2);
    }, [currentMonthData]);

    const allTimeTotal: string = useMemo(() => {
        const total: number = allTimeData.reduce((acc, item) => acc + Number(item.price || 0) * Number(item.quantity || 0), 0);
        return total.toFixed(2);
    }, [allTimeData]);

    return (
        <div
            style={{
                display: "flex",
                flexDirection: "column",
                width: "30%",
                height: "100%",
                padding: "10px",
                justifyContent: "space-around",
                alignItems: "stretch",
            }}
        >
            <h3>Inkomsten Deze Maand</h3>

            {isLoading && <p>Loading…</p>}
            {error && <p style={{color: "red"}}>Error: {error}</p>}

            <div style={{flex: "1 1 200px", minHeight: "200px"}}>
                <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                        <Pie
                            data={currentMonthData}
                            dataKey={(entry) => entry.price * entry.quantity}
                            nameKey="productName"
                            innerRadius="60%"
                            isAnimationActive={false}
                            fill="var(--primary)"
                        />
                        <Tooltip
                            formatter={(value) => typeof value === "number" ? "€ " + value.toFixed(2) : value}
                        />
                    </PieChart>
                </ResponsiveContainer>
            </div>

            <p>totaal: € {currentMonthTotal}</p>

            <h3>All Time Inkomsten</h3>

            <div style={{flex: "1 1 200px", minHeight: "200px"}}>
                <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                        <Pie
                            data={allTimeData}
                            dataKey={(entry) => entry.price * entry.quantity}
                            nameKey="productName"
                            outerRadius="80%"
                            innerRadius="60%"
                            isAnimationActive={false}
                            fill="var(--primary)"
                        />
                        <Tooltip
                            formatter={(value) => typeof value === "number" ? "€ " + value.toFixed(2) : value}
                        />
                    </PieChart>
                </ResponsiveContainer>
            </div>

            <p>totaal: € {allTimeTotal}</p>
        </div>
    );
}
