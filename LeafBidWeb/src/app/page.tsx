'use client';
import { useAuth } from "@/utils/useAuth";
import Header from "@/components/Header/Header";
import AuctionView from "@/components/Pages/AuctionView";
import AuctionDashboard from "@/components/Pages/AuctionDashboard";
import ProductDashboard from "@/components/Pages/ProductDashboard";

export default function HomePage() {
    const { user, loggedIn } = useAuth();

    if (loggedIn === null) {
        // wacht tot auth-status bekend is
        return <p>Loading...</p>;
    }

    const roles = user?.roles || [];

    const isBuyer = roles.includes("Buyer");
    const isAuctioneer = roles.includes("Auctioneer");
    const isProvider = roles.includes("Provider");
    const isAdmin = roles.includes("Admin");

    if (isBuyer) return (
        <>
            <Header />
            <AuctionView />
        </>
    );

    if (isAuctioneer) return (
        <>
            <Header />
            <AuctionDashboard />
        </>
    );

    if (isProvider) return (
        <>
            <Header />
            <ProductDashboard />
        </>
    );

    if (isAdmin) return (
        <>
            <Header />
            <AuctionView />
        </>
    );

    return (
        <>
            <Header />
            <p>Welkom! Geen rol toegewezen.</p>
        </>
    );
}
