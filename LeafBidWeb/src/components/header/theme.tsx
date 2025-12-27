'use client';

import {useEffect} from "react";

export function getTheme(): 'dark' | 'light' {
    const stored = localStorage.getItem('theme');
    return stored === 'dark' ? 'dark' : 'light';
}

export const toggleTheme = (): void => {
    const currentTheme = getTheme();
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
};

export function initializeTheme(): void {
    const theme = getTheme();
    document.documentElement.setAttribute('data-theme', theme);
}

export default function ThemeInitializer() {
    useEffect(() => {
        initializeTheme();
    }, []);

    return null;
}