'use client';

import { useEffect, useState } from 'react';

type Theme = 'dark' | 'light';

function isBrowser(): boolean
{
    return typeof window !== 'undefined' && typeof document !== 'undefined';
}

export function getTheme(): Theme
{
    if (!isBrowser())
    {
        return 'light';
    }

    const stored = window.localStorage.getItem('theme');
    return stored === 'dark' ? 'dark' : 'light';
}

export function setTheme(theme: Theme): void
{
    if (!isBrowser())
    {
        return;
    }

    document.documentElement.setAttribute('data-theme', theme);
    window.localStorage.setItem('theme', theme);
}

export function toggleTheme(): void
{
    if (!isBrowser())
    {
        return;
    }

    const currentTheme = getTheme();
    const newTheme: Theme = currentTheme === 'dark' ? 'light' : 'dark';
    setTheme(newTheme);
}

export function initializeTheme(): void
{
    if (!isBrowser())
    {
        return;
    }

    const theme = getTheme();
    setTheme(theme);
}

export default function ThemeInitializer()
{
    const [mounted, setMounted] = useState(false);

    useEffect(() =>
    {
        setMounted(true);
    }, []);

    useEffect(() =>
    {
        if (!mounted)
        {
            return;
        }

        initializeTheme();
    }, [mounted]);

    return null;
}
