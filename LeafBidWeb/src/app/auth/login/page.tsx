'use client'

import Image from 'next/image';
import Link from 'next/link';
import s from '../page.module.css';
import "bootstrap/dist/css/bootstrap-grid.min.css"
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import {useRouter} from 'next/navigation';
import React, {useState} from "react";
import LoginFailedException, {isLoginFailedException} from "@/exceptions/Auth/LoginFailedException";
import ValidationFailedException, {isValidationFailedException} from "@/exceptions/ValidationFailedException";
import {Login} from "@/types/User/Login";
import TextInput from "@/components/input/TextInput";

export default function LoginPage() {
    const router = useRouter();
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
            const response = await fetch("http://localhost:5001/api/v1/User/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
                body: JSON.stringify(loginData),
            });

            if (!response.ok) {
                throw LoginFailedException("login mislukt.");
            }

            const data = await response.json();
            window.accessToken = data.accessToken;

            if (typeof data.expiresIn === "number") {
                window.accessTokenExpiresAt = Date.now() + data.expiresIn * 1000;
            }
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
            newErrors.email = "Email is verplicht.";
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

                <h1 id="loginTitle" className={s.title}>Welkom bij Leafbid</h1>

                <Form noValidate className={s.form}>
                    {/* Email */}
                    <TextInput label={"email"} name={"email"} placeholder={"E-mail"} value={loginData.email}
                               onChange={(e) => {
                                   // Make sure there are only letters, digits, @ and . in the email
                                   e.target.value = e.target.value.replace(/[^a-zA-Z0-9@.]/g, '');
                                   setLoginData({...loginData, email: e.target.value})
                               }}/>

                    {/* Password */}
                    <TextInput label={"password"} name={"password"} placeholder={"Password"}
                               value={loginData.password}
                               onChange={(e) => setLoginData({...loginData, password: e.target.value})}
                               secret={true}/>

                    {/* Remember me */}
                    <Form.Label className={`form-check-label ${s.check}`} htmlFor="remember">
                        <Form.Control
                            className={`form-check-input ${s.checkInput} p-0`}
                            type="checkbox"
                            id="remember"
                            name="remember"
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
                    <Form.Control as={Button} type="submit" value="Inloggen"
                                  onClick={handleSubmit}>Inloggen</Form.Control>
                </Form>

                <p className={s.registerLine}>
                    <Link href="/auth/register" className={s.registerLink}>
                        Nog geen account?
                    </Link>
                </p>
            </section>
        </main>
    );
}