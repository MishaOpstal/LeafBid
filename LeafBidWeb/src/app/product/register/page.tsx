"use client";

import s from "@/app/layouts/add/page.module.css";
import ToevoegenLayout from "@/app/layouts/add/layout";
import Form from "react-bootstrap/Form";
import TextInput from "@/components/Input/TextInput";
import NumberInput from "@/components/Input/NumberInput";
import Button from "@/components/Input/Button";
import React, {useCallback, useEffect, useMemo, useState} from "react";
import SelectableButtonGroup from "@/components/Input/SelectableButtonGroup";
import SearchableDropdown from "@/components/Input/SearchableDropdown";
import DateSelect from "@/components/Input/DateSelect";
import {isUserInRole} from "@/utils/IsUserInRole";
import {parseRole, Roles} from "@/enums/Roles";
import {Product} from "@/types/Product/Product";

// Constants - defined outside component to prevent recreation on every render
const MEASUREMENT_OPTIONS = ["Pot grootte", "Stem lengte"];

// Check if a user has a Provider role
if (!isUserInRole(parseRole(Roles.Provider))) {
    // Redirect to dashboard
    if (typeof window !== 'undefined') {
        window.location.href = "/";
    }
}

export default function ProductForm() {
    const [formData, setFormData] = useState({
        minPrice: "", //required
        weight: "", //required
        region: "", //required
        potSize: "", //either required or stemLength
        stemLength: "", //either required or potSize
        measurementType: "Pot grootte", //Pot Size or Stem Length toggle
        stock: "", //required
        harvestedAt: "", //required
        productId: "", //required
    });

    let ProductId = null;
    if (typeof window !== 'undefined') {
        ProductId = localStorage.getItem("productId") ?? "";
    }

    const [products, setProducts] = useState<Product[]>([]);
    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [message, setMessage] = useState("");

    // Memoize sorted products to prevent recreation on every render
    const sortedProducts = useMemo(() => {
        return [...products].sort((a, b) => a.name.localeCompare(b.name));
    }, [products]);

    const fetchAvailableProducts = async () => {
        try {
            const res = await fetch("http://localhost:5001/api/v2/Product/available", {
                method: "GET",
                credentials: "include",
            });
            if (!res.ok) {
                throw new Error("Failed to fetch products");
            }

            const data: Product[] = await res.json();
            setProducts(data);
        } catch (error) {
            console.error("Error fetching products:", error);
        }
    }

    useEffect(() => {
        void fetchAvailableProducts();
    }, []);

    const validate = (): boolean => {
        const newErrors: Record<string, string> = {};
        if (!formData.productId) newErrors.productId = "Product productId is verplicht.";
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const {name, value} = e.target;
        setFormData((prev) => ({...prev, [name]: value}));
    };

    const handleProductSelect = (product: Product) => {
        setFormData((prev) => ({...prev, productId: product.id.toString()}));
        // Also update localStorage if that's where you intend to persist it for the submitForm call
        localStorage.setItem("productId", product.id.toString());
    };

    const handleDateSelect = (date: string | null) => {
        setFormData((prev) => ({...prev, harvestedAt: date ?? ""}));
    };

    const submitForm = async (e: React.FormEvent) => {
        e.preventDefault();
        setMessage("");

        if (!validate()) return;
        setIsSubmitting(true);

        try {


            const userData = localStorage.getItem("userData");
            const userId = userData ? JSON.parse(userData).id : null;

            const payload = {
                minPrice: parseFloat(formData.minPrice),
                weight: parseFloat(formData.weight),
                region: formData.region,
                potSize:
                    formData.measurementType === "Pot grootte"
                        ? parseFloat(formData.potSize)
                        : null,
                stemLength:
                    formData.measurementType === "Stem lengte"
                        ? parseFloat(formData.stemLength)
                        : null,
                stock: parseInt(formData.stock),
                harvestedAt: formData.harvestedAt,
                userId: userId,
            };


            const response = await fetch(`http://localhost:5001/api/v2/Product/registeredCreate/${ProductId}`, {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                credentials: "include",
                body: JSON.stringify(payload),
            });

            if (!response.ok) {
                throw new Error(`Server returned ${response.status}`);
            }

            setMessage("Product succesvol toegevoegd!");
            setFormData({
                minPrice: "",
                weight: "",
                region: "",
                potSize: "",
                stemLength: "",
                measurementType: "Pot grootte",
                stock: "",
                harvestedAt: "",
                productId: "",
            });
        } catch (error) {
            console.error(error);
            setMessage("Er is iets misgegaan bij het versturen van het formulier.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <ToevoegenLayout>
            <Form className={s.form} onSubmit={submitForm}>
                <h1>Product registreren</h1>

                <SearchableDropdown
                    label="Product"
                    items={sortedProducts}
                    displayKey="name"
                    valueKey="id"
                    value={formData.productId ? formData.productId : null}
                    onSelect={(product) => {
                        handleProductSelect(product);
                        setFormData((prev) => ({
                            ...prev,
                            productId: String(product.id),
                        }));
                    }}
                    placeholder="Selecteer product"
                />

                {errors.productId && <div className={s.error}>{errors.productId}</div>}

                <NumberInput
                    label="Aantal"
                    name="stock"
                    placeholder="aantal"
                    step={1}
                    min={1}
                    value={formData.stock}
                    onChange={handleChange}
                />
                {errors.stock && <div className={s.error}>{errors.stock}</div>}

                <NumberInput
                    label="Minimale Prijs"
                    name="minPrice"
                    placeholder="min. prijs"
                    step={0.01}
                    min={0.01}
                    value={formData.minPrice}
                    onChange={handleChange}
                    prefix="€"
                />
                {errors.minPrice && <div className={s.error}>{errors.minPrice}</div>}

                <NumberInput
                    label="Gewicht"
                    name="weight"
                    placeholder="gewicht"
                    step={0.01}
                    min={0.01}
                    value={formData.weight}
                    onChange={handleChange}
                    postfix="kg"
                />
                {errors.weight && <div className={s.error}>{errors.weight}</div>}

                <TextInput
                    label="Regio"
                    name="region"
                    placeholder="regio"
                    value={formData.region}
                    onChange={handleChange}
                />
                {errors.region && <div className={s.error}>{errors.region}</div>}

                <DateSelect
                    label="Oogst Datum"
                    placeholder="Selecteer startdatum"
                    onSelect={handleDateSelect}
                />
                {errors.harvestedAt && <div className={s.error}>{errors.harvestedAt}</div>}

                <SelectableButtonGroup
                    name="measurementType"
                    options={MEASUREMENT_OPTIONS}
                    value={formData.measurementType}
                    onChange={(name, value) =>
                        setFormData((prev) => ({...prev, [name]: value}))
                    }
                />
                {errors.measurementType && <div className={s.error}>{errors.measurementType}</div>}

                {formData.measurementType === "Pot grootte" && (
                    <>
                        <NumberInput
                            label="Pot grootte"
                            name="potSize"
                            placeholder="pot grootte"
                            step={0.1}
                            min={0.1}
                            value={formData.potSize}
                            onChange={handleChange}
                            postfix="cm"
                        />

                        {errors.potSize && <div className={s.error}>{errors.potSize}</div>}
                    </>
                )}

                {formData.measurementType === "Stem lengte" && (
                    <>
                        <NumberInput
                            label="Stam lengte"
                            name="stemLength"
                            placeholder="stam lengte"
                            step={0.1}
                            min={0.1}
                            value={formData.stemLength}
                            onChange={handleChange}
                            postfix="cm"
                        />

                        {errors.stemLength && <div className={s.error}>{errors.stemLength}</div>}
                    </>
                )}

                <Button
                    variant="primary"
                    type="submit"
                    label={isSubmitting ? "Versturen..." : "Aanmaken"}
                    disabled={isSubmitting}
                />

                {message && <p style={{marginTop: "1rem"}}>{message}</p>}
            </Form>
        </ToevoegenLayout>
    );
}