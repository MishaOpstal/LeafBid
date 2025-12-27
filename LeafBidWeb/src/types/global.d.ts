export {};

declare global {
    interface Window {
        accessToken?: string;
        accessTokenExpiresAt?: number; // unix ms
    }
}
