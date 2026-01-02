'use client';

import styles from '../../page.module.css';
import Header from "@/components/header/header";
import ActionButtons from "@/components/smallButton/smallButton";
import DashboardPanel from "@/components/dashboardPanel/dashboardpanel";
import React, { useState, useEffect } from "react";

type Auction = {
    id: number;
    startDate: string;
    clockLocationEnum: number;
    auctioneerId: number;
    products: Product[];
};

type Product = {
    id: number;
    name: string;
    description: string;
    minPrice: string;
    maxPrice: string;
    weight: number;
    picture: string;
    species: string;
    region: string;
    potSize: number;
    stemLength: number;
    stock: number;
    harvestedAt: string;
    providerId: number;
    auctionId: number;
};

type PageResponse = {
    auction: Auction;
    products: Product[];
};

const auctionIdList = [1, 2, 3, 4, 7]; // example IDs

export default function Home() {
    const [auctions, setAuctions] = useState<Auction[]>([]);
    const [loading, setLoading] = useState(true);

    const handleDelete = () => {
        // TODO : implement delete functionality
    };

    const handleUpdate = () => {
        // TODO : implement update functionality
    };

    useEffect(() => {
        const fetchAuctions = async () => {
            setLoading(true);
            try {
                const results = await Promise.all(
                    auctionIdList.map(async (id) => {
                        const res = await fetch(`http://localhost:5001/api/v2/Pages/${id}`);
                        if (!res.ok) return null;

                        const data: PageResponse = await res.json();

                        return {
                            ...data.auction,
                            products: data.products || [],
                        } as Auction;
                    })
                );

                const filtered = results.filter((a): a is Auction => a !== null);
                setAuctions(filtered);
            } catch (err) {
                console.error("Failed to load auctions:", err);
            } finally {
                setLoading(false);
            }
        };

        fetchAuctions();
    }, []);

    // Categorize auctions
    const now = new Date();
    const current = auctions.filter(a => new Date(a.startDate) <= now);
    const upcoming = auctions.filter(a => new Date(a.startDate) > now);
    const past: Auction[] = []; // TODO somehow add past auctions with something like a end date in database

    return (
        <>
            <Header returnOption={true} />
            <main className={styles.main}>
                <div className={styles.page}>
                    <h1>Alle veilingen</h1>

                    <h2 className={styles.padding}>Huidige veilingen</h2>
                    <div className={styles.panels}>
                        {loading ? (
                            <DashboardPanel
                                compact
                                loading
                                title="Placeholder title"
                                kloklocatie="Klok X"
                                resterendeTijd="--:--"
                                imageSrc="/images/PIPIPOTATO.png"
                            />
                        ) : current.length === 0 ? (
                            <p>Geen huidige veilingen beschikbaar</p>
                        ) : (
                            current.map((auction) => {
                                const product = auction.products[0];
                                return (
                                    <DashboardPanel
                                        key={auction.id}
                                        compact
                                        imageSrc={product?.picture ? `http://localhost:5001/uploads/${product.picture}` : "/images/placeholder.png"}
                                        kloklocatie={`Klok ${auction.clockLocationEnum}`}
                                        resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                    >
                                        <ActionButtons onDelete={handleDelete} onUpdate={handleUpdate} />
                                    </DashboardPanel>
                                );
                            })
                        )}
                    </div>

                    <h2 className={styles.padding}>Aankomende veilingen</h2>
                    <div className={styles.panels}>
                        {loading ? (
                            <DashboardPanel
                                compact
                                loading
                                title="Placeholder title"
                                kloklocatie="Klok X"
                                resterendeTijd="--:--"
                                imageSrc="/images/PIPIPOTATO.png"
                            />
                        ) : upcoming.length === 0 ? (
                            <p>Geen aankomende veilingen beschikbaar</p>
                        ) : (
                            upcoming.map((auction) => {
                                const product = auction.products[0];
                                return (
                                    <DashboardPanel
                                        key={auction.id}
                                        compact
                                        imageSrc={product?.picture ? `http://localhost:5001/uploads/${product.picture}` : "/images/placeholder.png"}
                                        kloklocatie={`Klok ${auction.clockLocationEnum}`}
                                        resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                    >
                                        <ActionButtons onDelete={handleDelete} onUpdate={handleUpdate} />
                                    </DashboardPanel>
                                );
                            })
                        )}
                    </div>

                    <h2 className={styles.padding}>Afgelopen veilingen</h2>
                    <div className={styles.panels}>
                        {loading ? (
                            <DashboardPanel
                                compact
                                loading
                                title="Placeholder title"
                                kloklocatie="Klok X"
                                resterendeTijd="--:--"
                                imageSrc="/images/PIPIPOTATO.png"
                            />
                        ) : past.length === 0 ? (
                            <p>Geen afgelopen veilingen beschikbaar</p>
                        ) : (
                            past.map((auction) => (
                                <DashboardPanel
                                    key={auction.id}
                                    compact
                                    imageSrc={"/images/placeholder.png"}
                                    kloklocatie={`Klok ${auction.clockLocationEnum}`}
                                    resterendeTijd={new Date(auction.startDate).toLocaleString()}
                                >
                                    <ActionButtons onDelete={handleDelete} onUpdate={handleUpdate} />
                                </DashboardPanel>
                            ))
                        )}
                    </div>
                </div>
            </main>
        </>
    );
}



// 'use client';
//
// import styles from '../page.module.css';
// import Header from "@/components/header/header";
// import ActionButtons from "@/components/smallButton/smallButton";
// import DashboardPanel from "@/components/dashboardPanel/dashboardpanel";
//
// export default function Home() {
//
//     const handleDelete = () => {
//         // TODO : implement delete functionality
//     };
//
//     const handleUpdate = () => {
//         // TODO : implement update functionality
//     };
//
//
//     return (
//         <>
//             <Header></Header>
//             <main className={styles.main}>
//
//                 <div className={styles.page}>
//                     <h1>Alle veilingen</h1>
//                     <h2>Huidige veilingen</h2>
//
//                     <DashboardPanel
//                         compact
//                         imageSrc="/images/PIPIPOTATO.png"
//                         kloklocatie="Klok 1 - Hal A"
//                         resterendeTijd="9 nov 2025, 16:45"
//                     >
//                         <ActionButtons onDelete={handleDelete} onUpdate={handleUpdate} />
//                     </DashboardPanel>
//
//
//
//                     <DashboardPanel
//                         compact
//                         imageSrc="/images/PIPIPOTATO.png"
//                         kloklocatie="Klok 2 - Kantine"
//                         resterendeTijd="10 nov 2025, 08:30"
//                     >
//                         <ActionButtons onDelete={handleDelete} onUpdate={handleUpdate} />
//                     </DashboardPanel>
//
//                     <h2>Aankomende veilingen</h2>
//                     <DashboardPanel
//                         compact
//                         imageSrc="/images/PIPIPOTATO.png"
//                         kloklocatie="Klok 3 - Vergaderzaal B"
//                         resterendeTijd="11 nov 2025, 12:00"
//                     >
//                         <ActionButtons onDelete={handleDelete} onUpdate={handleUpdate} />
//                     </DashboardPanel>
//
//                     <DashboardPanel
//                         compact
//                         imageSrc="/images/PIPIPOTATO.png"
//                         kloklocatie="Klok 4 - Receptie"
//                         resterendeTijd="12 nov 2025, 17:15"
//                     >
//                         <ActionButtons onDelete={handleDelete} onUpdate={handleUpdate} />
//                     </DashboardPanel>
//                     <h2>Afgelopen veilingen</h2>
//                     <DashboardPanel
//                         compact
//                         imageSrc="/images/PIPIPOTATO.png"
//                         kloklocatie="Klok 5 - Werkplaats"
//                         resterendeTijd="13 nov 2025, 09:45"
//                     >
//                         <ActionButtons onDelete={handleDelete} onUpdate={handleUpdate} />
//                     </DashboardPanel>
//
//                     <DashboardPanel
//                         compact
//                         imageSrc="/images/PIPIPOTATO.png"
//                         kloklocatie="Klok 4 - Receptie"
//                         resterendeTijd="12 nov 2025, 17:15"
//                     >
//                         <ActionButtons onDelete={handleDelete} onUpdate={handleUpdate} />
//                     </DashboardPanel>
//
//                 </div>
//
//             </main>
//
//         </>
//
//
//     );
// }
