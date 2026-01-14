let serverOffset = 0;

export const setServerTimeOffset = (serverTimeStr: string | Date) => {
    const serverTime = new Date(serverTimeStr).getTime();
    const clientTime = Date.now();
    serverOffset = serverTime - clientTime;
};

export const getServerNow = () => {
    return new Date(Date.now() + serverOffset);
};

export const getServerOffset = () => {
    return serverOffset;
};

export function parseDate(date: Date|string): string {
    const options: Intl.DateTimeFormatOptions = {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: 'numeric',
        minute: 'numeric',
    };

    const formatter = new Intl.DateTimeFormat('nl-NL', options);
    return formatter.format(new Date(date));
}
