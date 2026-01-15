'use client';

import { UseAuth } from "@/utils/UseAuth";
import Header from "@/components/Header/Header";
import AuctionView from "@/components/Pages/AuctionView";
import AuctionDashboard from "@/components/Pages/AuctionDashboard";
import ProductDashboard from "@/components/Pages/ProductDashboard";
import {useRouter} from "nextjs-toploader/app";

export default function HomePage() {
    const router = useRouter();
    const { user, loggedIn } = UseAuth();

    if (loggedIn === null) {
        return <p>Loading...</p>;
    }

    const roles = user?.roles || [];

    const isBuyer = roles.includes("Buyer");
    const isAuctioneer = roles.includes("Auctioneer");
    const isProvider = roles.includes("Provider");
    const isAdmin = roles.includes("Admin");

    if (isBuyer) {
        return (
            <>
                <Header />
                <AuctionView />
            </>
        );
    }

    if (isAuctioneer) {
        return (
            <>
                <Header />
                <AuctionDashboard />
            </>
        );
    }

    if (isProvider) {
        return (
            <>
                <Header />
                <ProductDashboard />
            </>
        );
    }

    if (isAdmin) {
        return (
            <>
                <Header />
                <AuctionView />
            </>
        );
    }

    router.push("/auth/login");
}
