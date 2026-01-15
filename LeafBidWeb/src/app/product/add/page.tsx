"use client";

import s from "@/app/layouts/add/page.module.css";
import ToevoegenLayout from "@/app/layouts/add/layout";
import Form from "react-bootstrap/Form";
import TextInput from "@/components/Input/TextInput";
import FileInput from "@/components/Input/FileInput";
import TextAreaInput from "@/components/Input/TextAreaInput";
import Button from "@/components/Input/Button";
import React, {useEffect, useState} from "react";
import {isUserInRole} from "@/utils/IsUserInRole";
import {parseRole, Roles} from "@/enums/Roles";

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

    // Check if a user has an Auctioneer role
    useEffect(() => {
        if (!isUserInRole(parseRole(Roles.Auctioneer))) {
            // Redirect to dashboard
            if (typeof window !== 'undefined') {
                window.location.href = "/";
            }
        }
    }, []);

    const validate = (): boolean => {
        const newErrors: Record<string, string> = {};
        if (!formData.name) newErrors.name = "Product naam is verplicht.";
        if (!formData.species) newErrors.species = "Soort is verplicht.";
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const {name, value} = e.target;
        setFormData((prev) => ({...prev, [name]: value}));
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
        setErrors((prev) => {
            const {...rest} = prev;
            return rest;
        });
        setFormData((prev) => ({...prev, picture: file}));
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

            const payload = {
                name: formData.name,
                description: formData.description,
                species: formData.species,
                picture: pictureBase64, // Base64 string or null
            };

            const response = await fetch("http://localhost:5001/api/v2/Product", {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                credentials: "include",
                body: JSON.stringify(payload),
            });

            if (!response.ok) {
                console.error("Failed to add product:", response.status);
                return;
            }

            setMessage("Product succesvol toegevoegd!");
            setFormData({
                name: "",
                species: "",
                picture: null,
                description: "",
            });
            setErrors({});
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
                <h1>Product aanmaken</h1>

                <TextInput
                    label="Product Naam"
                    name="name"
                    placeholder="naam"
                    value={formData.name}
                    onChange={handleChange}
                />
                {errors.name && <div className={s.error}>{errors.name}</div>}

                <TextInput
                    label="Soort"
                    name="species"
                    placeholder="soort"
                    value={formData.species}
                    onChange={handleChange}
                />
                {errors.species && <div className={s.error}>{errors.species}</div>}

                <FileInput label="Plaatje" name="picture" onChange={handleFileChange}/>
                {errors.picture && <div className={s.error}>{errors.picture}</div>}

                <TextAreaInput
                    label="Product Informatie"
                    name="description"
                    placeholder="product informatie"
                    value={formData.description}
                    onChange={handleChange}
                />
                {errors.description && <div className={s.error}>{errors.description}</div>}

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