'use client'

import Image from 'next/image';
import Link from 'next/link';
import s from '../page.module.css';
import "bootstrap/dist/css/bootstrap-grid.min.css"
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import {useRouter} from 'nextjs-toploader/app';
import React, {useState} from "react";
import LoginFailedException, {isLoginFailedException} from "@/exceptions/Auth/LoginFailedException";
import ValidationFailedException, {isValidationFailedException} from "@/exceptions/ValidationFailedException";
import {Login} from "@/types/User/Login";
import TextInput from "@/components/Input/TextInput";
import {toggleTheme} from "@/components/Header/Theme";
import {useAuth} from "@/utils/useAuth";

export default function LoginPage() {
    toggleTheme();

    const router = useRouter();
    const {checkAuth} = useAuth();

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const userNameRegex = /^[a-zA-Z0-9]+$/;

    const handleSubmit = async (e: React.FormEvent) => {
        try {
            await submitForm(e);
            router.push('/');
        } catch (error) {
            if (isLoginFailedException(error) || isValidationFailedException(error)) {
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
            const response = await fetch("http://localhost:5001/api/v2/User/login", {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                credentials: "include",
                body: JSON.stringify(loginData),
            });

            if (!response.ok) {
                throw LoginFailedException("login mislukt.");
            }

            await checkAuth({force: true});
        } finally {
            setIsSubmitting(false);
        }
    };

    const [loginData, setLoginData] = useState<Login>({
        email: "",
        password: "",
        remember: false
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [message, setMessage] = useState("");

    const validate = (): boolean => {
        const newErrors: Record<string, string> = {};

        if (!loginData.email) {
            newErrors.email = "E-mail is verplicht.";
        } else if (
            !emailRegex.test(loginData.email) &&
            !userNameRegex.test(loginData.email)
        ) {
            newErrors.email = "Voer een geldig e-mailadres of gebruikersnaam in.";
        }

        if (!loginData.password) {
            newErrors.password = "Wachtwoord is verplicht.";
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };


    return (
        <main className={s.main}>
            <title>Login</title>
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
                    {/* Email */}
                    <TextInput
                        name={"email"}
                        label={"E-mail or Username"}
                        placeholder={"no-reply@leafbid.nl"}
                        value={loginData.email}
                        onChange={(e) => {
                            const value = e.target.value.replace(/\s/g, '');
                            setLoginData({...loginData, email: value});
                        }}
                    />
                    {errors.email && <div className={s.error}>{errors.email}</div>}

                    {/* Password */}
                    <TextInput
                        name={"password"}
                        label={"Wachtwoord"}
                        placeholder={"Wachtwoord"}
                        value={loginData.password}
                        onChange={(e) => setLoginData({
                            ...loginData,
                            password: e.target.value
                        })}
                        secret={true}/>
                    {errors.password && <div className={s.error}>{errors.password}</div>}

                    {/* Remember me */}
                    <Form.Label className={`form-check-label ${s.check}`} htmlFor="remember">
                        <Form.Control
                            name="remember"
                            className={`form-check-input ${s.checkInput} p-0`}
                            type="checkbox"
                            id="remember"
                            checked={loginData.remember}
                            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                                setLoginData({
                                    ...loginData,
                                    remember: e.target.checked,
                                })
                            }
                        />
                        Onthoud mij?
                    </Form.Label>

                    {/* Submit */}
                    <Form.Control
                        as={Button}
                        type="submit"
                        value="Inloggen"
                        onClick={handleSubmit}
                        disabled={isSubmitting}
                    >Inloggen</Form.Control>
                </Form>

                {message && <div className={s.message}>{message}</div>}

                <p className={s.registerLine}>
                    <Link href="/auth/register" className={s.registerLink}>
                        Nog geen account?
                    </Link>
                </p>
            </section>
        </main>
    );
}