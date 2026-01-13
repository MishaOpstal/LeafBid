import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import ClientTopLoader from '@/components/clientTopLoader/ClientTopLoader';

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
    title: "LeafBid",
    description: "Veilingen voor jou favoriete groene producten",

    icons: {
        icon: [
            { url: "/icons/favicon.ico" },
            { url: "/icons/favicon-16x16.png", sizes: "16x16", type: "image/png" },
            { url: "/icons/favicon-32x32.png", sizes: "32x32", type: "image/png" },
            { url: "/icons/favicon-48x48.png", sizes: "48x48", type: "image/png" }
        ],

        apple: [
            { url: "/icons/apple-touch-icon-60x60.png", sizes: "60x60" },
            { url: "/icons/apple-touch-icon-72x72.png", sizes: "72x72" },
            { url: "/icons/apple-touch-icon-120x120.png", sizes: "120x120" },
            { url: "/icons/apple-touch-icon-152x152.png", sizes: "152x152" },
            { url: "/icons/apple-touch-icon-180x180.png", sizes: "180x180" }
        ],

        other: [
            {
                rel: "icon",
                url: "/icons/android-chrome-192x192.png",
                sizes: "192x192",
                type: "image/png"
            },
            {
                rel: "icon",
                url: "/icons/android-chrome-512x512.png",
                sizes: "512x512",
                type: "image/png"
            }
        ]
    },

    manifest: "/icons/site.webmanifest",

    other: {
        "msapplication-TileColor": "#da532c",
        "msapplication-TileImage": "/icons/mstile-150x150.png"
    }
};


export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="nl-NL" data-theme="light">
    <head>
        <link rel="preconnect" href="https://fonts.googleapis.com"/>
        <link rel="preconnect" href="https://fonts.gstatic.com"/>
        <link href="https://fonts.googleapis.com/css2?family=Nunito:ital,wght@0,200..1000;1,200..1000&display=swap"
              rel="stylesheet"/>
    </head>
    <body className={`${geistSans.variable} ${geistMono.variable}`} suppressHydrationWarning={true}>
    <ClientTopLoader />
    {children}
    </body>
    </html>
  );
}
