import {useRouter} from "nextjs-toploader/app";
import {useEffect, useState} from "react";
import {AuctionPageResult} from "@/types/Auction/AuctionPageResult";
import {setServerTimeOffset} from "@/utils/Time";
import config from "@/Config";

export const useAuctions = () => {
    const router = useRouter();
    const [auctions, setAuctions] = useState<AuctionPageResult[]>([]);
    const [loading, setLoading] = useState(true);

    // Fetch auctions from the server
    useEffect(() => {
        const loadAuctions = async (): Promise<void> => {
            setLoading(true);

            try {
                const res = await fetch(`${config.apiUrl}/Pages`, {
                    method: "GET",
                    credentials: "include"
                });

                if (!res.ok) {
                    if (res.status !== 404) {
                        console.error("Failed to fetch auctions");
                        return;
                    }
                    return;
                }

                const data: AuctionPageResult[] = await res.json();
                setAuctions(data);

                if (data.length > 0) {
                    setServerTimeOffset(data[0].serverTime);
                }
            } catch (err) {
                console.error("Failed to load auctions:", err);
            } finally {
                setLoading(false);
            }
        };

        void loadAuctions();
    }, []);
    return {router, auctions, loading}
}