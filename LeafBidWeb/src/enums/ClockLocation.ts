export enum ClockLocation {
    Naaldwijk,
    Aalsmeer,
    Rijnsburg,
    Eelde
}

export function parseClockLocation(clockLocation: number) {
    console.log(`parsing clock location for: ${clockLocation}`);

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