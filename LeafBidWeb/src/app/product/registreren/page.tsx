"use client";

import s from "@/app/layouts/toevoegen/page.module.css";
import ToevoegenLayout from "@/app/layouts/toevoegen/layout";
import Form from "react-bootstrap/Form";
import TextInput from "@/components/input/TextInput";
import FileInput from "@/components/input/FileInput";
import TextAreaInput from "@/components/input/TextAreaInput";
import Button from "@/components/input/Button";
import React, { useState } from "react";
import {isUserInRole} from "@/app/auth/utils/isUserInRole";

// Check if user has a Auctioneer role
if (!isUserInRole("Auctioneer") && !isUserInRole("Admin")) {
    // Redirect to dashboard
    window.location.href = "/";
}

export default function ProductForm() {
    const [formData, setFormData] = useState({

        name: "", //required
        description: "", //optional
        picture: null as File | null, //optional
        species: "", //required
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
                species: formData.species,
                picture: pictureBase64, // Base64 string or null
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
                species: "",
                picture: null,
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

                <TextInput
                    label="Soort"
                    name="species"
                    placeholder="soort"
                    value={formData.species}
                    onChange={handleChange}
                />

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