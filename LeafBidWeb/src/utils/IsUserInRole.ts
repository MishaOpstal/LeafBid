"use client";

import {parseRole, Roles} from "@/enums/Roles";

export const isUserInRole = (role: string): boolean => {
    let userData = null;
    if (typeof window !== 'undefined') {
        userData = localStorage.getItem("userData") ? JSON.parse(<string>localStorage.getItem("userData")) as {
            roles?: unknown
        } : null;
    }

    if (!userData) {
        return false;
    }

    try {
        if (!Array.isArray(userData.roles)) {
            return false;
        }

        // If our role is admin, it should always return true
        if (userData.roles.includes(parseRole(Roles.Admin))) {
            return true;
        }

        return userData.roles
            .filter((r): r is string => typeof r === "string" && r.trim().length > 0)
            .some((r) => r.toLowerCase() === role.trim().toLowerCase());
    } catch {
        return false;
    }
};
