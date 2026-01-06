'use client';

import s from "./header.module.css";
import Image from "next/image";
import Link from "next/link";
import {MoonFill, Sun} from "react-bootstrap-icons";
import ThemeInitializer, {getTheme, toggleTheme} from "./theme";
import {useEffect, useState} from "react";
import {LoggedInResponse} from "@/types/User/Auth/LoggedInResponse";
import {useRouter} from "next/navigation";

interface HeaderProps {
    returnOption?: boolean;
}

export default function Header({returnOption = false}: HeaderProps) {
    const router = useRouter();
    const [theme, setTheme] = useState<"dark" | "light">("light");
    const [isAuctioneer, setIsAuctioneer] = useState(false);
    const [isProvider, setIsProvider] = useState(false);

    useEffect(() => {
        setTheme(getTheme());
    }, []);

    // call fetchUserData on mount (top-level useEffect)
    useEffect(() => {
        fetchUserData();
    }, []);

    const onToggleTheme = () => {
        toggleTheme();
        setTheme(getTheme());
    };

    const logout = async (e?: React.MouseEvent) => {
        e?.preventDefault();

        try {
            const res = await fetch("http://localhost:5001/api/v2/User/logout", {
                method: "POST",
                credentials: "include",
                headers: {"Content-Type": "application/json"},
            });

            if (!res.ok) throw new Error("Logout mislukt");

            document.cookie = "session=; Max-Age=0; path=/";
            document.cookie = "PHPSESSID=; Max-Age=0; path=/";

            localStorage.removeItem("userData");
            localStorage.removeItem("loggedIn");

            window.location.href = "/auth/login";
        } catch (err) {
            console.error(err);
            window.location.href = "/auth/login";
        }
    };

    interface UserData {
        roles?: (string | null)[] | null;
        role?: string | null;
        isAuctioneer?: boolean | null;
        isProvider?: boolean | null;
    }

// replace the previous determineRoles(userData: any)
    const determineRoles = (userData?: UserData | null) => {
        if (!userData) return {auctioneer: false, provider: false};

        const lower = (v?: string | null) => (typeof v === "string" ? v.toLowerCase() : "");
        const rolesArray = Array.isArray(userData.roles)
            ? userData.roles
                .filter((r): r is string => typeof r === "string")
                .map((r) => r.toLowerCase())
            : [];

        const auctioneer =
            userData.isAuctioneer === true ||
            lower(userData.role) === "auctioneer" ||
            rolesArray.includes("auctioneer");

        const provider =
            userData.isProvider === true ||
            lower(userData.role) === "provider" ||
            rolesArray.includes("provider") ||
            rolesArray.includes("aanvoerder");

        return {auctioneer, provider};
    };


    const fetchUserData = () => {
        // Send request to localhost:5001/api/v2/User/me
        fetch("http://localhost:5001/api/v2/User/me", {
            method: "GET",
            credentials: "include",
        })
            .then((res) => res.json())
            .then((response: LoggedInResponse) => {
                if (response.loggedIn) {
                    // Set localStorage
                    localStorage.setItem("loggedIn", "true");
                    localStorage.setItem("userData", JSON.stringify(response.userData));
                    const {auctioneer, provider} = determineRoles(response.userData);
                    setIsAuctioneer(auctioneer);
                    setIsProvider(provider);
                } else {
                    // Remove localStorage
                    localStorage.setItem("loggedIn", "false");
                    localStorage.removeItem("userData");
                    setIsAuctioneer(false);
                    setIsProvider(false);
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
        } else {
            const stored = localStorage.getItem("userData");
            if (stored) {
                try {
                    const parsed = JSON.parse(stored);
                    const {auctioneer, provider} = determineRoles(parsed);
                    setIsAuctioneer(auctioneer);
                    setIsProvider(provider);
                } catch {
                    setIsAuctioneer(false);
                    setIsProvider(false);
                }
            }
        }
    };
    useEffect(() => {
        fetchUserData();
    }, []);
    return (
        <header>
            <ThemeInitializer/>
            <div className={s.logoWrapper}>
                <Image
                    src="/LeafBid.svg"
                    alt="LeafBid Logo"
                    fill
                    style={{objectFit: "contain"}}
                    priority
                    onClick={() => { window.location.href = "/"}}
                />
            </div>

            <nav aria-label="main navigation" className="user-select-none">

                {returnOption && (
                    <Link href="#" onClick={() => router.back()} className={s.link}>
                        Terug
                    </Link>
                )}
                <div className={s.clickables}>
                {isProvider && (
                    <Link href="/product/toevoegen" className={s.link}>
                        Product toevoegen
                    </Link>
                )}

                {isAuctioneer && (
                    <>
                        <Link href="/veiling/toevoegen" className={s.link}>
                            Veiling toevoegen
                        </Link>
                        <Link href="/veiling/veilingmeesterOverzicht" className={s.link}>
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
