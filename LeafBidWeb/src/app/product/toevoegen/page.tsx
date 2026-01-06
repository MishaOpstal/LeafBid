"use client";

import s from "@/app/layouts/toevoegen/page.module.css";
import ToevoegenLayout from "@/app/layouts/toevoegen/layout";

import Form from "react-bootstrap/Form";
import TextInput from "@/components/input/TextInput";
import NumberInput from "@/components/input/NumberInput";
import FileInput from "@/components/input/FileInput";
import TextAreaInput from "@/components/input/TextAreaInput";
import Button from "@/components/input/Button";
import React, { useState } from "react";
import SelectableButtonGroup from "@/components/input/SelectableButtonGroup";
import DateSelect from "@/components/input/DateSelect";
import {isUserInRole} from "@/app/auth/utils/isUserInRole";
import {Roles, parseRole} from "@/enums/Roles";

// Check if user has a Provider role
if (!isUserInRole(parseRole(Roles.Provider)) && !isUserInRole(parseRole(Roles.Admin))) {
    // Redirect to dashboard
    window.location.href = "/";
}

export default function ProductForm() {
    const [formData, setFormData] = useState({
        name: "", //required
        minPrice: "", //required
        weight: "", //required
        species: "", //required
        region: "", //required
        potSize: "", //either required or stemLength
        stemLength: "", //either required or potSize
        measurementType: "Pot grootte", //Pot Size or Stem Length toggle
        stock: "", //required
        harvestedAt: "", //required
        userId: "1", //required
        description: "", //optional
        picture: null as File | null, //optional
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [message, setMessage] = useState("");

    const validate = (): boolean => {
        const newErrors: Record<string, string> = {};
        if (!formData.name) newErrors.name = "Product naam is verplicht.";
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
            let pictureBase64: string | null = null;

            if (formData.picture) {
                pictureBase64 = await fileToBase64(formData.picture);
            }

            const userData = localStorage.getItem("userData");
            const userId = userData ? JSON.parse(userData).id : null;

            const payload = {
                name: formData.name,
                description: formData.description,
                minPrice: parseFloat(formData.minPrice),
                weight: parseFloat(formData.weight),
                species: formData.species,
                region: formData.region,
                picture: pictureBase64, // Base64 string or null
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
                name: "",
                minPrice: "",
                weight: "",
                species: "",
                region: "",
                picture: null,
                potSize: "",
                stemLength: "",
                measurementType: "Pot grootte",
                stock: "",
                harvestedAt: "",
                userId: userId,
                description: "",
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

                <TextInput
                    label="Product Naam"
                    name="name"
                    placeholder="naam"
                    value={formData.name}
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
                    label="Soort"
                    name="species"
                    placeholder="soort"
                    value={formData.species}
                    onChange={handleChange}
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

                <FileInput label="Plaatje" name="picture" onChange={handleFileChange} />

                <TextAreaInput
                    label="Product Informatie"
                    name="description"
                    placeholder="product informatie"
                    value={formData.description}
                    onChange={handleChange}
                />

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
