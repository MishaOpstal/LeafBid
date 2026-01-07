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
