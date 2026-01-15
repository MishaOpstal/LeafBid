"use client";

import Header from "@/components/Header/Header";
import AuctionView from "@/components/Pages/AuctionView";
import {isUserInRole} from "@/utils/IsUserInRole";
import {parseRole, Roles} from "@/enums/Roles";

export default function AuctionViewPage() {

    // Check if a user has a Buyer role
    if (!isUserInRole(parseRole(Roles.Buyer))) {
        // Redirect to dashboard
        if (typeof window !== 'undefined') {
            window.location.href = "/";
        }
    }

    return (
        <>
            <Header/>
            <AuctionView/>
        </>
    );
}
