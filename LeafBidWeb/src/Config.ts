const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5001";

const config = {
    apiUrl: `${apiBaseUrl}/api/v2`,
    hubUrl: `${apiBaseUrl}/auctionHub`,
    baseUrl: apiBaseUrl,
};

export default config;
