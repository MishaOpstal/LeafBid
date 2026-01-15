import Header from "@/components/Header/Header";
import s from './layout.module.css';
import React from "react";

export default function RootLayout({
                                       children,
                                   }: Readonly<{
    children: React.ReactNode;
}>) {
    return (
        <>
            <Header returnOption={true}/>
            <main className={s.main}>{children}</main>
        </>
    );
}