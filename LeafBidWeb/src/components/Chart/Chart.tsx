"use client";
import {useEffect, useState} from "react";
import {Pie, PieChart, Tooltip} from "recharts";

interface SaleChartDataPoint {
    productName: string;
    price: number;

    [key: string]: string | number;
}

interface SaleChartResponse {
    currentMonthData: SaleChartDataPoint[];
    allTimeData: SaleChartDataPoint[];
}

export default function Chart() {
    const [chartData, setChartData] = useState<SaleChartResponse | null>(null);
    useEffect(() => {
        const fetchChartData = async () => {
            try {
                const res = await fetch("http://localhost:5001/api/v2/AuctionSaleProduct/chart", {
                    method: "GET",
                    credentials: "include",
                });
                const data = await res.json();
                setChartData(data);
            } catch (error) {
                console.error("Error fetching Chart data:", error);
            }
        };
        fetchChartData().then(() => console.log("Fetched Chart data"));
    }, []);
    console.log("Chart data:", chartData?.currentMonthData);
    return (
        <div
            style={{
                display: 'flex',
                flexDirection: 'column',
                width: '30%',
                height: '100%',
                padding: '10px',
                justifyContent: 'space-around',
                alignItems: 'stretch',
            }}
        >
            <h3>Inkomsten Deze Maand</h3>
            <PieChart responsive style={{flex: '1 1 200px', aspectRatio: 1}}>
                <Pie data={chartData?.currentMonthData} dataKey="price" nameKey="productName" innerRadius="60%"
                     isAnimationActive={false} fill={"var(--primary"}/>
                <Tooltip/>
            </PieChart>
            <p>totaal: € {chartData?.currentMonthData.reduce((acc, item) => acc + item.price, 0).toFixed(2)}</p>
            <h3>All Time Inkomsten</h3>
            <PieChart responsive style={{flex: '1 1 200px', aspectRatio: 1}}>
                <Pie data={chartData?.allTimeData} dataKey="price" nameKey="productName" outerRadius="80%"
                     innerRadius="60%" isAnimationActive={false} fill={"var(--primary"}/>
                <Tooltip/>
            </PieChart>
            <p>totaal: € {chartData?.allTimeData.reduce((acc, item) => acc + item.price, 0).toFixed(2)}</p>

        </div>
    );
}
