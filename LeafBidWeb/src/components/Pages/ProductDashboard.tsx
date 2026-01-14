"use client";

import ProductTable from "@/components/ProductTable/ProductTable";
import {parseRole, Roles} from "@/enums/Roles";
import Chart from "@/components/Chart/Chart";
import {isUserInRole} from "@/utils/IsUserInRole";
import {useAuth} from "@/utils/useAuth";
import React from "react";
import {router} from "next/dist/client";


export default function ProductDashboard() {
    const {user} = useAuth();
    if (user && !isUserInRole(parseRole(Roles.Provider)) && !isUserInRole(parseRole(Roles.Buyer)) && !isUserInRole(parseRole(Roles.Admin))) {
        void router.push("/");
    }
    return (
        <>
            <main style={{height: 'calc(100dvh - 100px)'}}
                  className="d-flex flex-row justify-content-center align-items-center p-4">
                {user && isUserInRole(parseRole(Roles.Provider)) && user.companyId !== null && (
                    <>
                        <Chart/>
                        <ProductTable userRoles={Roles.Provider}/>
                    </>
                )}

                {user && isUserInRole(parseRole(Roles.Buyer)) && (
                    <ProductTable userRoles={Roles.Buyer}/>
                )}
                {user && !isUserInRole(parseRole(Roles.Provider)) && !isUserInRole(parseRole(Roles.Buyer))}
            </main>
        </>
    );
}