export enum ClockLocation {
    Naaldwijk,
    Aalsmeer,
    Rijnsburg,
    Eelde
}

export function parseClockLocation(clockLocation: number) {
    switch (clockLocation) {
        case ClockLocation.Naaldwijk:
            return "Naaldwijk";
        case ClockLocation.Aalsmeer:
            return "Aalsmeer";
        case ClockLocation.Rijnsburg:
            return "Rijnsburg";
        case ClockLocation.Eelde:
            return "Eelde";
        default:
            return "Unknown";
    }
}