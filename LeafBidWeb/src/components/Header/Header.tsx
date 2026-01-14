"use client";

import s from "./Header.module.css";
import Image from "next/image";
import Link from "next/link";
import { MoonFill, Sun } from "react-bootstrap-icons";
import ThemeInitializer, { getTheme, toggleTheme } from "./Theme";
import React, { useCallback, useEffect, useState, useRef } from "react";
import { LoggedInResponse } from "@/types/User/Auth/LoggedInResponse";
import { useRouter } from "nextjs-toploader/app";
import { isUserInRole } from "@/utils/IsUserInRole";
import { parseRole, Roles } from "@/enums/Roles";
import {useAuth} from "@/utils/useAuth";

interface HeaderProps {
    returnOption?: boolean;
}

export default function Header({ returnOption = false }: HeaderProps) {
    const { user, logout } = useAuth();
    const router = useRouter();

    return (
        <header>
            <ThemeInitializer />

            <div className={s.logoWrapper}>
                <Image
                    src="/LeafBid.svg"
                    alt="LeafBid Logo"
                    fill
                    style={{ objectFit: "contain" }}
                    priority
                    onClick={() => router.push("/")}
                />
            </div>

            <nav className="user-select-none">
                {returnOption && (
                    <button type="button" onClick={() => router.back()} className={s.link}>
                        Terug
                    </button>
                )}

                <div className={s.navigationContainer}>
                    {user && isUserInRole(parseRole(Roles.Provider)) && (
                        <Link href="/product/register" className={s.link}>
                            Product registreren
                        </Link>
                    )}

                    {user && isUserInRole(parseRole(Roles.Auctioneer)) && (
                        <>
                            <Link href="/product/add" className={s.link}>Product aanmaken</Link>
                            <Link href="/auction/add" className={s.link}>Veiling toevoegen</Link>
                            <Link href="/auction/dashboard" className={s.link}>Veilingmeester overzicht</Link>
                        </>
                    )}

                    {user && (
                        <button type="button" onClick={logout} className={s.link}>
                            Uitloggen
                        </button>
                    )}
                </div>
            </nav>
        </header>
    );
}