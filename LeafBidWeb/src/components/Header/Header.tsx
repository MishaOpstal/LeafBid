'use client';

import s from "./Header.module.css";
import Image from "next/image";
import Link from "next/link";
import { MoonFill, Sun } from "react-bootstrap-icons";
import ThemeInitializer, { getTheme, toggleTheme } from "./Theme";
import React, { useCallback, useEffect, useState } from "react";
import { LoggedInResponse } from "@/types/User/Auth/LoggedInResponse";
import { useRouter } from "nextjs-toploader/app";
import { isUserInRole } from "@/utils/IsUserInRole";
import { parseRole, Roles } from "@/enums/Roles";

interface HeaderProps {
    returnOption?: boolean;
}

export default function Header({ returnOption = false }: HeaderProps) {
    const router = useRouter();
    const [theme, setTheme] = useState<"dark" | "light">("light");

    useEffect(() => {
        setTheme(getTheme());
    }, []);

    const onToggleTheme = () => {
        toggleTheme();
        setTheme(getTheme());
    };

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

            document.cookie = "session=; Max-Age=0; path=/";
            document.cookie = "PHPSESSID=; Max-Age=0; path=/";

            localStorage.removeItem("userData");
            localStorage.removeItem("loggedIn");

            router.push("/auth/login");
        } catch (err) {
            console.error(err);
            router.push("/auth/login");
        }
    }, [router]);

    useEffect(() => {
        const checkLoggedInState = async () => {
            if (localStorage.getItem("loggedIn") === "false") {
                await logout();
            }
        };

        fetch("http://localhost:5001/api/v2/User/me", {
            method: "GET",
            credentials: "include",
        })
            .then((res) => res.json())
            .then((response: LoggedInResponse) => {
                if (response.loggedIn) {
                    localStorage.setItem("loggedIn", "true");
                    localStorage.setItem("userData", JSON.stringify(response.userData));
                } else {
                    localStorage.setItem("loggedIn", "false");
                    localStorage.removeItem("userData");
                }
            })
            .finally(async () => {
                await checkLoggedInState();
            })
            .catch((err) => {
                console.error(err);
            });
    }, [logout]);

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
                    onClick={() => {
                        router.push("/");
                    }}
                />
            </div>

            <nav aria-label="main navigation" className="user-select-none">
                {returnOption && (
                    <Link href="#" onClick={() => router.back()} className={s.link}>
                        Terug
                    </Link>
                )}

                <div className={s.navigationContainer}>
                    {isUserInRole(parseRole(Roles.Provider)) && (
                        <Link href="/product/register" className={s.link}>
                            Product registreren
                        </Link>
                    )}

                    {isUserInRole(parseRole(Roles.Auctioneer)) && (
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

                    <Link href="#" onClick={logout} className={s.link}>
                        Uitloggen
                    </Link>

                    {theme === "light" ? (
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
                    )}
                </div>
            </nav>
        </header>
    );
}
