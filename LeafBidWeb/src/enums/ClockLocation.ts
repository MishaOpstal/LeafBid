export enum ClockLocation {
    Aalsmeer,
    Eelde,
    Naaldwijk,
    Rijnsburg
}

export function parseClockLocation(clockLocation: number) {
    switch (clockLocation) {
        case ClockLocation.Aalsmeer:
            return "Aalsmeer";
        case ClockLocation.Eelde:
            return "Eelde";
        case ClockLocation.Naaldwijk:
            return "Naaldwijk";
        case ClockLocation.Rijnsburg:
            return "Rijnsburg";
        default:
            return "Unknown";
    }
}