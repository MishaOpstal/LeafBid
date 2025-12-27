export interface User {
    lastLogin: string|null;
    id: string;
    userName: string;
    normalizedUserName: string;
    email: string;
    normalizedEmail: string;
    emailConfirmed: boolean;
    lockoutEnd: string|null;
    lockoutEnabled: boolean;
    accessFailedCount: number;
    roles: string[]|null;
}