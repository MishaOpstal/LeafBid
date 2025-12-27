export const isUserInRole = (role: string): boolean => {
    const raw = localStorage.getItem("userData");
    if (!raw) return false;

    try {
        const userData = JSON.parse(raw) as { roles?: unknown };

        if (!Array.isArray(userData.roles)) return false;

        return userData.roles
            .filter((r): r is string => typeof r === "string" && r.trim().length > 0)
            .some((r) => r.toLowerCase() === role.trim().toLowerCase());
    } catch {
        return false;
    }
};
