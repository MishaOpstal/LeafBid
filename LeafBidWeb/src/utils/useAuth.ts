import { useState, useEffect, useCallback } from "react";
import { useRouter } from "nextjs-toploader/app";
import { LoggedInResponse } from "@/types/User/Auth/LoggedInResponse";

export function useAuth() {
    const router = useRouter();
    const [user, setUser] = useState<LoggedInResponse["userData"] | null>(null);
    const [loggedIn, setLoggedIn] = useState<boolean | null>(null);

    const logout = useCallback(async () => {
        try {
            const res = await fetch("http://localhost:5001/api/v2/User/logout", {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
            });

            if (!res.ok) throw new Error("Logout mislukt");
        } catch (err) {
            console.error("Logout error:", err);
        } finally {
            document.cookie = "session=; Max-Age=0; path=/";
            document.cookie = "PHPSESSID=; Max-Age=0; path=/";

            localStorage.removeItem("userData");
            localStorage.removeItem("loggedIn");

            setUser(null);
            setLoggedIn(false);

            router.push("/auth/login");
        }
    }, [router]);

    const checkAuth = useCallback(async () => {
        if (localStorage.getItem("loggedIn") === "false") {
            await logout();
            return;
        }

        try {
            const res = await fetch("http://localhost:5001/api/v2/User/me", {
                method: "GET",
                credentials: "include",
            });
            const response: LoggedInResponse = await res.json();

            if (response.loggedIn) {
                localStorage.setItem("loggedIn", "true");
                localStorage.setItem("userData", JSON.stringify(response.userData));
                setUser(response.userData);
                setLoggedIn(true);
            } else {
                localStorage.setItem("loggedIn", "false");
                localStorage.removeItem("userData");
                await logout();
            }
        } catch (err) {
            console.error("Auth check failed:", err);
            if (localStorage.getItem("loggedIn") === "false") {
                await logout();
            }
        }
    }, [logout]);

    useEffect(() => {
        checkAuth();
    }, [checkAuth]);

    return { user, loggedIn, logout, checkAuth };
}
