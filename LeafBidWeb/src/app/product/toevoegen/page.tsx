"use client";

import s from "@/app/layouts/toevoegen/page.module.css";
import ToevoegenLayout from "@/app/layouts/toevoegen/layout";
import Form from "react-bootstrap/Form";
import TextInput from "@/components/input/TextInput";
import NumberInput from "@/components/input/NumberInput";
import Button from "@/components/input/Button";
import React, { useState } from "react";
import SelectableButtonGroup from "@/components/input/SelectableButtonGroup";
import DateSelect from "@/components/input/DateSelect";
import {isUserInRole} from "@/app/auth/utils/isUserInRole";

// Check if user has a Provider role
if (!isUserInRole("Provider") && !isUserInRole("Admin")) {
    // Redirect to dashboard
    window.location.href = "/";
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

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [message, setMessage] = useState("");

    const validate = (): boolean => {
        const newErrors: Record<string, string> = {};
        if (!formData.productId) newErrors.productId = "Product productId is verplicht.";
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setFormData((prev) => ({ ...prev, [name]: value }));
    };

    const handleDateSelect = (date: string | null) => {
        setFormData((prev) => ({ ...prev, harvestedAt: date ?? "" }));
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0] ?? null;
        if (file && !file.type.startsWith("image/")) {
            setErrors((prev) => ({
                ...prev,
                picture: "Alleen afbeeldingsbestanden zijn toegestaan.",
            }));
            return;
        }
        setFormData((prev) => ({ ...prev, picture: file }));
    };

    const fileToBase64 = (file: File): Promise<string> =>
        new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => {
                if (typeof reader.result === "string") resolve(reader.result);
                else reject("Bestand kon niet worden geconverteerd naar Base64.");
            };
            reader.onerror = () => reject("Fout bij het lezen van bestand.");
            reader.readAsDataURL(file);
        });

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

            const response = await fetch("http://localhost:5001/api/v2/Product", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
                body: JSON.stringify(payload),
            });

            const data = await response.json();

            if (!response.ok) throw new Error(`Server returned ${response.status}`);

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
                <h1>Product Toevoegen</h1>

                <NumberInput
                    label="Product id"
                    name="productId"
                    placeholder="product id"
                    step={1}
                    value={formData.productId}
                    onChange={handleChange}
                />

                <NumberInput
                    label="Aantal"
                    name="stock"
                    placeholder="aantal"
                    step={1}
                    value={formData.stock}
                    onChange={handleChange}
                />

                <NumberInput
                    label="Minimale Prijs"
                    name="minPrice"
                    placeholder="min. prijs"
                    step={0.01}
                    value={formData.minPrice}
                    onChange={handleChange}
                    prefix="€"
                />

                <NumberInput
                    label="Gewicht"
                    name="weight"
                    placeholder="gewicht"
                    step={0.01}
                    value={formData.weight}
                    onChange={handleChange}
                    postfix="kg"
                />

                <TextInput
                    label="Regio"
                    name="region"
                    placeholder="regio"
                    value={formData.region}
                    onChange={handleChange}
                />

                <DateSelect
                    label="Oogst Datum"
                    placeholder="Selecteer startdatum"
                    onSelect={handleDateSelect}
                />

                <SelectableButtonGroup
                    name="measurementType"
                    options={["Pot grootte", "Stem lengte"]}
                    value={formData.measurementType}
                    onChange={(name, value) =>
                        setFormData((prev) => ({ ...prev, [name]: value }))
                    }
                />

                {formData.measurementType === "Pot grootte" && (
                    <NumberInput
                        label="Pot grootte"
                        name="potSize"
                        placeholder="pot grootte"
                        step={0.1}
                        value={formData.potSize}
                        onChange={handleChange}
                        postfix="cm"
                    />
                )}

                {formData.measurementType === "Stem lengte" && (
                    <NumberInput
                        label="Stam lengte"
                        name="stemLength"
                        placeholder="stam lengte"
                        step={0.1}
                        value={formData.stemLength}
                        onChange={handleChange}
                        postfix="cm"
                    />
                )}

                <Button
                    variant="primary"
                    type="submit"
                    label={isSubmitting ? "Versturen..." : "Aanmaken"}
                    disabled={isSubmitting}
                />

                {message && <p style={{ marginTop: "1rem" }}>{message}</p>}
            </Form>
        </ToevoegenLayout>
    );
}
