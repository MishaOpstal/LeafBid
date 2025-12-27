'use client';

import s from "./header.module.css";
import Image from "next/image";
import Link from "next/link";
import { MoonFill, Sun } from "react-bootstrap-icons";
import ThemeInitializer, { getTheme, toggleTheme } from "./theme";
import { useEffect, useState } from "react";
import {LoggedInResponse} from "@/types/User/Auth/LoggedInResponse";

interface HeaderProps {
    returnOption?: boolean;
}

export default function Header({ returnOption = false }: HeaderProps) {
    const [theme, setTheme] = useState<"dark" | "light">("light");

    useEffect(() => {
        setTheme(getTheme());
    }, []);

    const onToggleTheme = () => {
        toggleTheme();
        setTheme(getTheme());
    };

    const logout = async (e?: React.MouseEvent) => {
        e?.preventDefault();

        try {
            const res = await fetch("http://localhost:5001/api/v1/User/logout", {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
            });

            if (!res.ok) throw new Error("Logout mislukt");

            document.cookie = "session=; Max-Age=0; path=/";
            document.cookie = "PHPSESSID=; Max-Age=0; path=/";

            localStorage.removeItem("userData");

            window.location.href = "/auth/login";
        } catch (err) {
            console.error(err);
            window.location.href = "/auth/login";
        }
    };

    const fetchUserData = () => {
        // Send request to localhost:5001/api/v1/User/me
        fetch("http://localhost:5001/api/v1/User/me", {
            method: "GET",
            credentials: "include",
        })
            .then((res) => res.json())
            .then((response: LoggedInResponse) => {
                if (response.loggedIn) {
                    // Set localStorage
                    localStorage.setItem("loggedIn", "true");
                    localStorage.setItem("userData", JSON.stringify(response.userData));
                } else {
                    // Remove localStorage
                    localStorage.setItem("loggedIn", "false");
                    localStorage.removeItem("userData");
                }
            })
            .finally(() => {
                checkLoggedInState();
            })
            .catch((err) => {
                console.error(err);
            });
    };

    const checkLoggedInState = () => {
        if (
            localStorage.getItem("loggedIn") &&
            localStorage.getItem("loggedIn") === "false"
        ) {
            logout();
        }
    }

    useEffect(() => {
        fetchUserData();
    }, []);

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
                />
            </div>

            <nav aria-label="main navigation" className="user-select-none">
                {returnOption && (
                    <Link href="/" className={s.link}>
                        Terug
                    </Link>
                )}

                <div className={s.clickables}>
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
