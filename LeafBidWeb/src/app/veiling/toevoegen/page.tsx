"use client";

import s from "@/app/layouts/toevoegen/page.module.css";
import ToevoegenLayout from "@/app/layouts/toevoegen/layout";

import Form from "react-bootstrap/Form";
import OrderedMultiSelect from "@/components/input/OrderedMultiSelect";
import { Product } from "@/types/Product/Product";
import { Locatie } from "@/types/Auction/Locatie";
import React, {useEffect, useState} from "react";
import DateSelect from "@/components/input/DateSelect";
import SearchableDropdown from "@/components/input/SearchableDropdown";
import Button from "@/components/input/Button";
import ProductPriceTable from "@/components/input/ProductPriceTable";
import { Auction } from "@/types/Auction/Auction";

const locaties: Locatie[] = [
    { locatieId: 1, locatieNaam: "Aalsmeer" },
    { locatieId: 2, locatieNaam: "Rijnsburg" },
    { locatieId: 3, locatieNaam: "Naaldwijk" },
    { locatieId: 4, locatieNaam: "Eelde" },
];

export default function Home() {
    const [auctionData, setAuctionData] = useState<Auction>({
        startDate: "",
        clockLocationEnum: 0,
        products: [] as Product[],
        userId: "8a57bc69-eeaa-42d1-930e-8270419f0a82",
        isLive: false,
        isVisible: false,
    });

    const [products, setProducts] = useState<Product[]>([]);
    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [message, setMessage] = useState("");

    useEffect(() => {
        (async () => {
            try {
                const res = await fetch("http://localhost:5001/api/v2/Product/available", {
                    method: "GET",
                    credentials: "include",
                });
                if (!res.ok) throw new Error("Failed to fetch products");
                const data: Product[] = await res.json();
                setProducts(data);
            } catch (error) {
                console.error("Error fetching products:", error);
            }
        })();
    }, []);

    const validate = (): boolean => {
        const newErrors: Record<string, string> = {};

        if (!auctionData.startDate) newErrors.startDate = "Startdatum en tijd is verplicht.";
        if (!auctionData.clockLocationEnum || auctionData.clockLocationEnum === 0)
            newErrors.location = "Locatie is verplicht.";
        if (!auctionData.products || auctionData.products.length === 0)
            newErrors.products = "Minimaal één product is verplicht.";

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleDateSelect = (date: string | null) => {
        setAuctionData((prev) => ({ ...prev, startDate: date ?? "" }));
    };

    const handleLocatieSelect = (loc: Locatie) => {
        setAuctionData((prev) => ({
            ...prev,
            clockLocationEnum: loc.locatieId, // or map to your enum if needed
        }));
    };

    const handleProductsSelect = (selected: Product[]) => {
        setAuctionData((prev) => ({ ...prev, products: selected }));
    };

    const handlePriceUpdate = (updated: Product[]) => {
        setAuctionData((prev) => ({ ...prev, products: updated }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setMessage("");
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            // Submit auctionData directly
            const response = await fetch("http://localhost:5001/api/v2/Auction", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(auctionData),
                credentials: "include",
            });

            const data = await response.json().catch(() => null);
            if (!response.ok) {
                throw new Error(data?.message ?? `Server returned ${response.status}`);
            }

            setMessage("Veiling succesvol aangemaakt!");
            setAuctionData({
                startDate: "",
                clockLocationEnum: 0,
                products: [],
                userId: "8a57bc69-eeaa-42d1-930e-8270419f0a82",
                isLive: false,
                isVisible: false,
            });
            setErrors({});
        } catch (err) {
            console.error(err);
            setMessage("Aanmaken mislukt. Controleer je invoer of probeer opnieuw.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <ToevoegenLayout>
            <Form className={s.form} onSubmit={handleSubmit}>
                <div className={s.multiRow}>
                    <section className={s.section}>
                        <h1 className={s.h1}>Veiling Aanmaken</h1>

                        <Form.Label>Startdatum en tijd</Form.Label>
                        <DateSelect
                            placeholder="Selecteer startdatum"
                            onSelect={handleDateSelect}
                            useTime={true}
                            label=""
                        />
                        {errors.startDate && <div className={s.error}>{errors.startDate}</div>}

                        <Form.Label>Locatie</Form.Label>
                        <SearchableDropdown
                            label="Selecteer locatie"
                            items={locaties}
                            displayKey="locatieNaam"
                            valueKey="locatieId"
                            onSelect={handleLocatieSelect}
                            placeholder="Zoek locatie..."
                        />
                        {errors.location && <div className={s.error}>{errors.location}</div>}

                        <ProductPriceTable
                            products={auctionData.products}
                            onChange={handlePriceUpdate}
                            height={300}
                        />
                        {errors.products && <div className={s.error}>{errors.products}</div>}
                    </section>

                    <section className={s.section}>
                        <h3 className={s.h3}>Gekoppelde Producten</h3>
                        <OrderedMultiSelect
                            items={products}
                            value={auctionData.products}
                            onChange={handleProductsSelect}
                            showBadges={false}
                        />
                    </section>
                </div>

                {message && <div className={s.message}>{message}</div>}

                <Button
                    variant="primary"
                    type="submit"
                    label={isSubmitting ? "Bezig..." : "Aanmaken"}
                    disabled={isSubmitting}
                />
            </Form>
        </ToevoegenLayout>
    );
}
