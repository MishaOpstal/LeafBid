"use client";

import Header from "@/components/Header/Header";
import AuctionDashboard from "@/components/Pages/AuctionDashboard";
import {isUserInRole} from "@/utils/IsUserInRole";
import {parseRole, Roles} from "@/enums/Roles";
export default function AuctionViewPage() {

    // Check if a user has an Auctioneer role
    if (!isUserInRole(parseRole(Roles.Auctioneer))) {
        // Redirect to dashboard
        if (typeof window !== 'undefined') {
            window.location.href = "/";
        }
    }

    return (
        <>
            <Header/>
            <AuctionDashboard/>
        </>
    );
}
