"use client";

import s from "./Header.module.css";
import Image from "next/image";
import Link from "next/link";
import { MoonFill, Sun } from "react-bootstrap-icons";
import ThemeInitializer, { getTheme, toggleTheme } from "./Theme";
import React, { useEffect, useState } from "react";
import { useRouter } from "nextjs-toploader/app";
import { isUserInRole } from "@/utils/IsUserInRole";
import { parseRole, Roles } from "@/enums/Roles";
import { useAuth } from "@/utils/useAuth";

interface HeaderProps
{
    returnOption?: boolean;
}

type Theme = "dark" | "light";

export default function Header({ returnOption = false }: HeaderProps)
{
    const { user, logout } = useAuth();
    const router = useRouter();

    const [mounted, setMounted] = useState(false);
    const [theme, setTheme] = useState<Theme>("light");

    useEffect(() =>
    {
        setMounted(true);
        setTheme(getTheme());
    }, []);

    const onToggleTheme = () =>
    {
        toggleTheme();
        setTheme(getTheme());
    };

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

                    {mounted && (
                        theme === "light" ? (
                            <Sun
                                title="Switch to dark mode"
                                onClick={onToggleTheme}
                                className={s.themeToggle}
                                role="button"
                                tabIndex={0}
                            />
                        ) : (
                            <MoonFill
                                title="Switch to light mode"
                                onClick={onToggleTheme}
                                className={s.themeToggle}
                                role="button"
                                tabIndex={0}
                            />
                        )
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
