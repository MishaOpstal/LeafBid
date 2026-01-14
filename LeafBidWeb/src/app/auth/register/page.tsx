'use client'

import {useRouter} from 'nextjs-toploader/app';
import React, {useState} from "react";
import s from '../page.module.css';
import "bootstrap/dist/css/bootstrap-grid.min.css"

import Image from 'next/image';
import Link from 'next/link';
import Form from "react-bootstrap/Form";

import Button from "@/components/Input/Button";
import RegisterFailedException, {isRegisterFailedException} from '@/exceptions/Auth/RegisterFailedException';
import SearchableDropdown from "@/components/Input/SearchableDropdown";
import RoleFetchFailedException from "@/exceptions/Auth/RoleFetchFailedException";
import TextInput from "@/components/Input/TextInput";
import ValidationFailedException, {isValidationFailedException} from "@/exceptions/ValidationFailedException";
import CompanyFetchFailedException, {isCompanyFetchFailedException} from "@/exceptions/Company/CompanyFetchFailedException";

import {Role} from "@/types/User/Role";
import {Company} from "@/types/Company/Company";
import {Register} from "@/types/User/Register";


const roles: Role[] = [];
const companies: Company[] = [];

// Retrieve roles from the backend using fetch
const fetchRoles = async () => {
    const response = await fetch("http://localhost:5001/api/v2/Role", {
        method: "GET",
        headers: {"Content-Type": "application/json"},
    });

    if (!response.ok) {
        throw RoleFetchFailedException("Failed to fetch roles.");
    }

    const data = await response.json();
    roles.push(...data);
}



fetchRoles().catch(error => {
    if (isRegisterFailedException(error)) {
        console.error(error.getMessage());
    } else {
        console.error("An unexpected error occurred while fetching roles.");
    }
});

// Retrieve companies from the backend using fetch
const fetchCompanies = async () => {
    const response = await fetch("http://localhost:5001/api/v2/Company", {
        method: "GET",
        headers: {"Content-Type": "application/json"},
    });

    if (!response.ok) {
        throw CompanyFetchFailedException("Failed to fetch companies.");
    }
    const data = await response.json();
    companies.push(...data);
}

fetchCompanies().catch(error => {
    if (isRegisterFailedException(error)) {
        console.error(error.getMessage());
    } else {
        console.error("An unexpected error occurred while fetching companies.");
    }
});


export default function RegisterPage() {
    const router = useRouter();
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    const handleSubmit = async (e: React.FormEvent) => {
        try {
            await submitForm(e);
            router.push('/auth/login');
        } catch (error) {
            if (isRegisterFailedException(error) || isValidationFailedException(error)) {
                setMessage(error.getMessage());
            } else {
                setMessage("Er is een onverwachte fout opgetreden.");
            }
            return;
        }
    };

    const submitForm = async (e: React.FormEvent) => {
        e.preventDefault();
        setMessage("");

        if (!validate()) {
            throw ValidationFailedException("Validatie mislukt.");
        }

        setIsSubmitting(true);

        try {
            const response = await fetch("http://localhost:5001/api/v2/User/register", {
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

    const [registerData, setRegisterData] = useState<Register>({
        userName: "",
        email: "",
        password: "",
        passwordConfirmation: "",
        roles: [],
        companyId: null
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [message, setMessage] = useState("");

    const validate = (): boolean => {
        const newErrors: Record<string, string> = {};
        if (!registerData.userName) {
            newErrors.name = "naam is verplicht.";
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

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
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

                <Form noValidate className={s.form}>
                    {/* Username */}
                    <TextInput
                        name={"userName"}
                        label={"Gebruikersnaam"}
                        placeholder={"KhalilBouter123"}
                        value={registerData.userName}
                        onChange={(e) => {
                            // Make sure there are only letters or digits in the username
                            e.target.value = e.target.value.replace(/[^a-zA-Z0-9]/g, '');
                            setRegisterData({...registerData, userName: e.target.value})
                        }}/>
                    {errors.userName && <div className={s.error}>{errors.userName}</div>}

                    {/* Email */}
                    <TextInput
                        name={"email"}
                        label={"E-mail"}
                        placeholder={"no-reply@leafbid.nl"}
                        value={registerData.email}
                        onChange={(e) => {
                            const value = e.target.value.replace(/\s/g, '');
                            setRegisterData({...registerData, email: value});
                        }}
                    />
                    {errors.email && <div className={s.error}>{errors.email}</div>}

                    {/* Password */}
                    <div className={s.passwordRow}>
                        <TextInput
                            name={"password"}
                            label={"Wachtwoord"}
                            placeholder={"Wachtwoord"}
                            value={registerData.password}
                            onChange={(e) => setRegisterData({
                                ...registerData,
                                password: e.target.value
                            })}
                            secret={true}/>
                        {errors.password && <div className={s.error}>{errors.password}</div>}

                        {/* Password Verify */}
                        <TextInput
                            name={"passwordConfirmation"}
                            label={"Wachtwoord verifiëren"}
                            placeholder={"Wachtwoord verifiëren"}
                            value={registerData.passwordConfirmation}
                            onChange={(e) => setRegisterData({
                                ...registerData,
                                passwordConfirmation: e.target.value
                            })} secret={true}/>
                        {errors.passwordConfirmation && <div className={s.error}>{errors.passwordConfirmation}</div>}
                    </div>

                    <SearchableDropdown
                        label="Selecteer rol"
                        placeholder="Zoek naar rol..."
                        items={roles}
                        displayKey="name"
                        valueKey="id"
                        onSelect={(e) => setRegisterData({
                            ...registerData,
                            roles: [e.name]
                        })}
                    />

                    {errors.roles && <div className={s.error}>{errors.roles}</div>}

                    {(
                        <SearchableDropdown
                            label="Selecteer een bedrijf"
                            placeholder="Zoek naar bedrijf..."
                            items={companies}
                            displayKey="name"
                            valueKey="id"
                            onSelect={(company) => {
                                setRegisterData({
                                    ...registerData,
                                    companyId: company.id
                                });
                            }}
                        />
                    )}

                    {/* Submit */}
                    <Button
                        variant="primary"
                        type="button"
                        label="Registreren"
                        onClick={handleSubmit}
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
