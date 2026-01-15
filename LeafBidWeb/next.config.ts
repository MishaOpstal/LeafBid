import type { NextConfig } from "next";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5001";
const apiUrlParsed = new URL(apiBaseUrl);

const nextConfig: NextConfig = {
    output: 'standalone',
    images: {
        remotePatterns: [
            {
                protocol: apiUrlParsed.protocol.replace(':', '') as 'http' | 'https',
                hostname: apiUrlParsed.hostname,
                port: apiUrlParsed.port,
                pathname: '/**',
            },
            {
                protocol: 'https',
                hostname: 'placehold.co',
                pathname: '/**',
            }
        ]
    }
};

export default nextConfig;
