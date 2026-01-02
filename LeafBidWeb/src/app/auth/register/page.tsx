'use client'

import Image from 'next/image';
import Link from 'next/link';
import s from '../page.module.css';
import "bootstrap/dist/css/bootstrap-grid.min.css"
import Form from "react-bootstrap/Form";
import Button from "@/components/input/Button";
import {useRouter} from 'next/navigation';
import React, {useState} from "react";
import {Register} from "@/types/User/Register";
import RegisterFailedException, {isRegisterFailedException} from '@/exceptions/Auth/RegisterFailedException';
import SearchableDropdown from "@/components/input/SearchableDropdown";
import {Role} from "@/types/User/Role";
import RoleFetchFailedException from "@/exceptions/Auth/RoleFetchFailedException";
import TextInput from "@/components/input/TextInput";
import ValidationFailedException, {isValidationFailedException} from "@/exceptions/ValidationFailedException";

const roles: Role[] = [];

// Retrieve roles from backend using fetch
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

export default function RegisterPage() {
    const router = useRouter();
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
        roles: []
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

                <h1 id="loginTitle" className={s.title}>Welkom bij Leafbid</h1>

                <Form noValidate className={s.form}>
                    {/* Username */}
                    <TextInput label={"username"} name={"userName"} placeholder={"Naam"} value={registerData.userName}
                               onChange={(e) => {
                                   // Make sure there are only letters or digits in the username
                                   e.target.value = e.target.value.replace(/[^a-zA-Z0-9]/g, '');
                                   setRegisterData({...registerData, userName: e.target.value})
                               }}/>

                    {/* Email */}
                    <TextInput label={"email"} name={"email"} placeholder={"E-mail"} value={registerData.email}
                               onChange={(e) => {
                                   // Make sure there are only letters, digits, @ and . in the email
                                   e.target.value = e.target.value.replace(/[^a-zA-Z0-9@.]/g, '');
                                   setRegisterData({...registerData, email: e.target.value})
                               }}/>

                    {/* Password */}
                    <div className={s.passwordRow}>
                        <TextInput label={"password"} name={"password"} placeholder={"Password"}
                                   value={registerData.password}
                                   onChange={(e) => setRegisterData({...registerData, password: e.target.value})}
                                   secret={true}/>

                        {/* Password Verify */}
                        <TextInput label={"passwordConfirmation"} name={"passwordConfirmation"}
                                   placeholder={"Bevestig Wachtwoord"}
                                   value={registerData.passwordConfirmation}
                                   onChange={(e) => setRegisterData({
                                       ...registerData,
                                       passwordConfirmation: e.target.value
                                   })} secret={true}/>
                    </div>

                    <SearchableDropdown
                        label="Selecteer rol"
                        items={roles}
                        displayKey="name"
                        valueKey="id"
                        onSelect={(e) => setRegisterData({
                            ...registerData,
                            roles: [e.name]
                        })}
                        placeholder="Zoek naar rol..."
                    />

                    {/* Submit */}
                    <Button
                        variant="primary"
                        type="button"
                        label="Registreren"
                        onClick={handleSubmit}
                    />
                </Form>

                <p className={s.registerLine}>
                    <Link href="/auth/login" className={s.registerLink}>
                        Al een account?
                    </Link>
                </p>
            </section>
        </main>
    );
}
