'use client';

import React, {useEffect, useMemo, useState} from "react";
import {useRouter} from "nextjs-toploader/app";
import Image from "next/image";
import Link from "next/link";
import Form from "react-bootstrap/Form";
import "bootstrap/dist/css/bootstrap-grid.min.css";

import s from "../page.module.css";

import Button from "@/components/Input/Button";
import TextInput from "@/components/Input/TextInput";
import SearchableDropdown from "@/components/Input/SearchableDropdown";

import RegisterFailedException, {isRegisterFailedException} from "@/exceptions/Auth/RegisterFailedException";
import RoleFetchFailedException, {isRoleFetchFailedException} from "@/exceptions/Auth/RoleFetchFailedException";
import CompanyFetchFailedException, {
    isCompanyFetchFailedException
} from "@/exceptions/Company/CompanyFetchFailedException";
import ValidationFailedException, {isValidationFailedException} from "@/exceptions/ValidationFailedException";

import {Role} from "@/types/User/Role";
import {Company} from "@/types/Company/Company";
import {Register} from "@/types/User/Register";
import config from "@/Config";

export default function RegisterPage() {
    const router = useRouter();
    const emailRegex = useMemo(() => /^[^\s@]+@[^\s@]+\.[^\s@]+$/, []);

    const [roles, setRoles] = useState<Role[]>([]);
    const [companies, setCompanies] = useState<Company[]>([]);

    const [selectedRoleId, setSelectedRoleId] = useState<string | number | null>(null);
    const [selectedCompanyId, setSelectedCompanyId] = useState<string | number | null>(null);

    const [registerData, setRegisterData] = useState<Register>({
        userName: "",
        email: "",
        password: "",
        passwordConfirmation: "",
        roles: [],
        companyId: null,
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
    const [message, setMessage] = useState<string>("");

    useEffect(() => {
        const fetchRoles = async () => {
            const response = await fetch(`${config.apiUrl}/Role`, {
                method: "GET",
                headers: {"Content-Type": "application/json"},
            });

            if (!response.ok) {
                throw RoleFetchFailedException("Failed to fetch roles.");
            }

            const data: Role[] = await response.json();
            setRoles(data);
        };

        const fetchCompanies = async () => {
            const response = await fetch(`${config.apiUrl}/Company`, {
                method: "GET",
                headers: {"Content-Type": "application/json"},
            });

            if (!response.ok) {
                throw CompanyFetchFailedException("Failed to fetch companies.");
            }

            const data: Company[] = await response.json();
            setCompanies(data);
        };

        fetchRoles().catch((error: unknown) => {
            if (isRoleFetchFailedException(error)) {
                console.error(error.getMessage());
            } else {
                console.error("An unexpected error occurred while fetching roles.", error);
            }
        });

        fetchCompanies().catch((error: unknown) => {
            if (isCompanyFetchFailedException(error)) {
                console.error(error.getMessage());
            } else {
                console.error("An unexpected error occurred while fetching companies.", error);
            }
        });
    }, []);

    const validate = (): boolean => {
        const newErrors: Record<string, string> = {};

        if (!registerData.userName) {
            newErrors.userName = "Naam is verplicht.";
        }

        if (!registerData.email) {
            newErrors.email = "Email is verplicht.";
        } else if (!emailRegex.test(registerData.email)) {
            newErrors.email = "Voer een geldig e-mailadres in.";
        }

        if (!registerData.password) {
            newErrors.password = "Wachtwoord is verplicht.";
        }

        if (registerData.password !== registerData.passwordConfirmation) {
            newErrors.passwordConfirmation = "Wachtwoorden komen niet overeen.";
        }

        if (registerData.userName.includes(" ")) {
            newErrors.userName = "Naam mag geen spaties bevatten.";
        }

        if (registerData.roles!.length === 0) {
            newErrors.roles = "Rol is verplicht.";
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const submitForm = async () => {
        setMessage("");

        if (!validate()) {
            throw ValidationFailedException("Validatie mislukt.");
        }

        setIsSubmitting(true);

        try {
            const response = await fetch(`${config.apiUrl}/User/register`, {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                body: JSON.stringify(registerData),
            });

            if (!response.ok) {
                throw RegisterFailedException("Registratie mislukt.");
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        try {
            await submitForm();
            router.push("/auth/login");
        } catch (error: unknown) {
            if (isRegisterFailedException(error) || isValidationFailedException(error)) {
                setMessage(error.getMessage());
            } else {
                setMessage("Er is een onverwachte fout opgetreden.");
                console.error(error);
            }
        }
    };

    return (
        <main className={s.main}>
            <title>Register</title>

            <section className={s.card} aria-labelledby="loginTitle">
                <div className={s.logoRow}>
                    <Image
                        src="/LeafBid.svg"
                        alt="Leafbid logo"
                        width={100}
                        height={100}
                        priority
                    />
                </div>

                <h1 id="loginTitle" className={s.title}>Welkom bij LeafBid</h1>

                <Form noValidate className={s.form} onSubmit={handleSubmit}>
                    <TextInput
                        name={"userName"}
                        label={"Gebruikersnaam"}
                        placeholder={"KhalilBouter123"}
                        value={registerData.userName}
                        onChange={(e) => {
                            const sanitized = e.target.value.replace(/[^a-zA-Z0-9]/g, "");
                            setRegisterData({...registerData, userName: sanitized});
                        }}
                    />
                    {errors.userName && <div className={s.error}>{errors.userName}</div>}

                    <TextInput
                        name={"email"}
                        label={"E-mail"}
                        placeholder={"no-reply@leafbid.nl"}
                        value={registerData.email}
                        onChange={(e) => {
                            const value = e.target.value.replace(/\s/g, "");
                            setRegisterData({...registerData, email: value});
                        }}
                    />
                    {errors.email && <div className={s.error}>{errors.email}</div>}

                    <div className={s.passwordRow}>
                        <TextInput
                            name={"password"}
                            label={"Wachtwoord"}
                            placeholder={"Wachtwoord"}
                            value={registerData.password}
                            onChange={(e) => setRegisterData({...registerData, password: e.target.value})}
                            secret={true}
                        />
                        {errors.password && <div className={s.error}>{errors.password}</div>}

                        <TextInput
                            name={"passwordConfirmation"}
                            label={"Wachtwoord verifiëren"}
                            placeholder={"Wachtwoord verifiëren"}
                            value={registerData.passwordConfirmation}
                            onChange={(e) => setRegisterData({...registerData, passwordConfirmation: e.target.value})}
                            secret={true}
                        />
                        {errors.passwordConfirmation && <div className={s.error}>{errors.passwordConfirmation}</div>}
                    </div>

                    <SearchableDropdown
                        label="Selecteer rol"
                        placeholder="Zoek naar rol..."
                        items={roles}
                        displayKey="name"
                        valueKey="id"
                        value={selectedRoleId}
                        onSelect={(role) => {
                            setSelectedRoleId(role.id as string | number);
                            setRegisterData({
                                ...registerData,
                                roles: [role.name],
                            });
                        }}
                    />
                    {errors.roles && <div className={s.error}>{errors.roles}</div>}

                    <SearchableDropdown
                        label="Selecteer een bedrijf"
                        placeholder="Zoek naar bedrijf..."
                        items={companies}
                        displayKey="name"
                        valueKey="id"
                        value={selectedCompanyId}
                        onSelect={(company) => {
                            setSelectedCompanyId(company.id as string | number);
                            setRegisterData({
                                ...registerData,
                                companyId: company.id,
                            });
                        }}
                    />

                    <Button
                        variant="primary"
                        type="submit"
                        label="Registreren"
                        disabled={isSubmitting}
                    />
                </Form>

                {message && <div className={s.message}>{message}</div>}

                <p className={s.registerLine}>
                    <Link href="/auth/login" className={s.registerLink}>
                        Al een account?
                    </Link>
                </p>
            </section>
        </main>
    );
}
