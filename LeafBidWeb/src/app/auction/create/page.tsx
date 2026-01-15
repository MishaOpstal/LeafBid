"use client";

import s from "@/app/layouts/add/page.module.css";
import ToevoegenLayout from "@/app/layouts/add/layout";

import Form from "react-bootstrap/Form";
import OrderedMultiSelect from "@/components/Input/OrderedMultiSelect";
import {Location} from "@/types/Auction/Location";
import React, {useEffect, useState, useCallback, useMemo} from "react";
import DateSelect from "@/components/Input/DateSelect";
import SearchableDropdown from "@/components/Input/SearchableDropdown";
import Button from "@/components/Input/Button";
import ProductPriceTable from "@/components/Input/ProductPriceTable";
import {Auction} from "@/types/Auction/Auction";
import {RegisteredProduct, RegisteredProductForAuction} from "@/types/Product/RegisteredProducts";
import {isUserInRole} from "@/utils/IsUserInRole";
import {parseRole, Roles} from "@/enums/Roles";

// Check if a user has an Auctioneer role
if (!isUserInRole(parseRole(Roles.Auctioneer))) {
    // Redirect to dashboard
    if (typeof window !== 'undefined') {
        window.location.href = "/";
    }
}

const locaties: Location[] = [
    {locatieId: 0, locatieNaam: "Aalsmeer"},
    {locatieId: 1, locatieNaam: "Eelde"},
    {locatieId: 2, locatieNaam: "Naaldwijk"},
    {locatieId: 3, locatieNaam: "Rijnsburg"},
];

const createEmptyAuction = (): Auction => ({
    startDate: "",
    clockLocationEnum: -1,
    registeredProducts: [] as RegisteredProduct[]
});

export default function Home() {
    const [auctionData, setAuctionData] = useState<Auction>(() => createEmptyAuction());
    const [registeredProducts, setRegisteredProducts] = useState<RegisteredProduct[]>([]);
    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [message, setMessage] = useState("");

    useEffect(() => {
        (async () => {
            try {
                const res = await fetch("http://localhost:5001/api/v2/Product/available/registered", {
                    method: "GET",
                    credentials: "include",
                });
                if (!res.ok) {
                    throw new Error("Failed to fetch products");
                }

                const data: RegisteredProduct[] = await res.json();

                // Fill the missing productId and companyId fields for all retrieved registered products, they reside inside the product -> id and company -> id respectively
                data.forEach(rp => {
                    if (rp.product) {
                        rp.productId = rp.product.id;
                    }
                    if (rp.company) {
                        rp.companyId = rp.company.id;
                    }
                });

                setRegisteredProducts(data);
            } catch (error) {
                console.error("Error fetching products:", error);
            }
        })();
    }, []);

    const validate = (): boolean => {
        const newErrors: Record<string, string> = {};

        if (!auctionData.startDate) {
            newErrors.startDate = "Startdatum en tijd is verplicht.";
        }

        if (auctionData.clockLocationEnum === -1) {
            newErrors.location = "Locatie is verplicht.";
        }

        if (!auctionData.registeredProducts || auctionData.registeredProducts.length === 0) {
            newErrors.products = "Minimaal één product is verplicht.";
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    // Memoize handlers to prevent unnecessary re-renders
    const handleDateSelect = useCallback((date: string | null) => {
        setAuctionData((prev) => ({...prev, startDate: date ?? ""}));
    }, []);

    const handleLocatieSelect = useCallback((loc: Location) => {
        setAuctionData((prev) => ({
            ...prev,
            clockLocationEnum: loc.locatieId,
        }));
    }, []);

    const handleProductsSelect = useCallback((selected: RegisteredProduct[]) => {
        setAuctionData((prev) => ({...prev, registeredProducts: selected}));
    }, []);

    const handlePriceUpdate = useCallback((updated: RegisteredProduct[]) => {
        setAuctionData((prev) => ({...prev, registeredProducts: updated}));
    }, []);

    const handleSubmit = useCallback(async (e: React.FormEvent) => {
        e.preventDefault();
        setMessage("");
        if (!validate()) {
            return;
        }

        setIsSubmitting(true);
        try {
            // Remove the product and company fields from registeredProducts before submission
            const registeredProductsForAuction: RegisteredProductForAuction[]
                = auctionData.registeredProducts.map((rp) => ({
                id: rp.id,
                maxPrice: rp.maxPrice,
            }));

            const submissionData = {
                ...auctionData,
                registeredProductsForAuction: registeredProductsForAuction,
            };

            // Submit auctionData directly
            const response = await fetch("http://localhost:5001/api/v2/Auction", {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                body: JSON.stringify(submissionData),
                credentials: "include",
            });

            const data = await response.json().catch(() => null);
            if (!response.ok) {
                throw new Error(data?.message ?? `Server returned ${response.status}`);
            }

            setMessage("Veiling succesvol aangemaakt!");
            setErrors({});

            location.reload();
        } catch (err) {
            console.error(err);
            setMessage("Aanmaken mislukt. Controleer je invoer of probeer opnieuw.");
        } finally {
            setIsSubmitting(false);
        }
    }, [auctionData, validate]); // Note: validate function needs to be stable or memoized too

    // Memoize sorted products to avoid re-sorting on every render
    const sortedRegisteredProducts = useMemo(() => {
        return [...registeredProducts].sort((a, b) =>
            a.product!.name.localeCompare(b.product!.name)
        );
    }, [registeredProducts]);

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
                            disallowPast={true}
                            label=""
                        />
                        {errors.startDate && <div className={s.error}>{errors.startDate}</div>}

                        <Form.Label>Locatie</Form.Label>
                        <SearchableDropdown
                            label="Selecteer locatie"
                            items={locaties}
                            displayKey="locatieNaam"
                            valueKey="locatieId"
                            value={auctionData.clockLocationEnum !== -1 ? auctionData.clockLocationEnum : null}
                            onSelect={(loc) => {
                                handleLocatieSelect(loc);
                            }}
                            placeholder="Selecteer locatie"
                        />
                        {errors.location && <div className={s.error}>{errors.location}</div>}

                        <ProductPriceTable
                            registeredProducts={auctionData.registeredProducts}
                            onChange={handlePriceUpdate}
                            height={300}
                        />
                        {errors.products && <div className={s.error}>{errors.products}</div>}
                    </section>

                    <section className={s.section}>
                        <h3 className={s.h3}>Gekoppelde Producten</h3>
                        <OrderedMultiSelect
                            items={sortedRegisteredProducts}
                            value={auctionData.registeredProducts}
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