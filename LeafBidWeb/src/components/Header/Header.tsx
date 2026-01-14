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

interface HeaderProps {
    returnOption?: boolean;
}

export default function Header({ returnOption = false }: HeaderProps) {
    const router = useRouter();

    const [mounted, setMounted] = useState<boolean>(false);
    const [theme, setTheme] = useState<"dark" | "light">("light");
    const hasCheckedAuth = useRef<boolean>(false); // Prevent multiple auth checks

    useEffect(() => {
        setMounted(true);
        setTheme(getTheme());
    }, []);

    const onToggleTheme = useCallback(() => {
        toggleTheme();
        setTheme(getTheme());
    }, []);

    const logout = useCallback(async (e?: React.MouseEvent) => {
        e?.preventDefault();

        try {
            const res = await fetch("http://localhost:5001/api/v2/User/logout", {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
            });

            if (!res.ok) {
                throw new Error("Logout mislukt, error: " + (await res.json()) + "");
            }
        } catch (err) {
            console.error(err);
        } finally {
            document.cookie = "session=; Max-Age=0; path=/";
            document.cookie = "PHPSESSID=; Max-Age=0; path=/";

            localStorage.removeItem("userData");
            localStorage.removeItem("loggedIn");

            router.push("/auth/login");
        }
    }, [router]);

    // Only check auth status once when component mounts
    useEffect(() => {
        if (!mounted || hasCheckedAuth.current) {
            return;
        }

        hasCheckedAuth.current = true;

        const checkAuthStatus = async () => {
            // Quick check: if localStorage says not logged in, logout immediately
            if (localStorage.getItem("loggedIn") === "false") {
                await logout();
                return;
            }

            // Otherwise, verify with server
            try {
                const res = await fetch("http://localhost:5001/api/v2/User/me", {
                    method: "GET",
                    credentials: "include",
                });

                const response: LoggedInResponse = await res.json();

                if (response.loggedIn) {
                    localStorage.setItem("loggedIn", "true");
                    localStorage.setItem("userData", JSON.stringify(response.userData));
                } else {
                    localStorage.setItem("loggedIn", "false");
                    localStorage.removeItem("userData");
                    await logout();
                }
            } catch (err) {
                console.error("Auth check failed:", err);
                // On error, check localStorage state
                if (localStorage.getItem("loggedIn") === "false") {
                    await logout();
                }
            }
        };

        checkAuthStatus();
    }, [mounted, logout]);

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

            <nav aria-label="main navigation" className="user-select-none">
                {returnOption && (
                    <button
                        type="button"
                        onClick={() => router.back()}
                        className={s.link}
                    >
                        Terug
                    </button>
                )}

                <div className={s.navigationContainer}>
                    {mounted && isUserInRole(parseRole(Roles.Provider)) && (
                        <Link href="/product/register" className={s.link}>
                            Product registreren
                        </Link>
                    )}

                    {mounted && isUserInRole(parseRole(Roles.Auctioneer)) && (
                        <>
                            <Link href="/product/add" className={s.link}>
                                Product aanmaken
                            </Link>
                            <Link href="/auction/add" className={s.link}>
                                Veiling toevoegen
                            </Link>
                            <Link href="/auction/dashboard" className={s.link}>
                                Veilingmeester overzicht
                            </Link>
                        </>
                    )}

                    <button
                        type="button"
                        onClick={logout}
                        className={s.link}
                    >
                        Uitloggen
                    </button>

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
                </div>
            </nav>
        </header>
    );
}