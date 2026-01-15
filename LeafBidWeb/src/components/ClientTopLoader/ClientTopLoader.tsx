'use client';

import NextTopLoader from 'nextjs-toploader';
import {JSX, useEffect, useState} from 'react';

export default function ClientTopLoader(): JSX.Element {
    const [color, setColor] = useState<string>('#22c55e');

    useEffect(() => {
        const readPrimary = (): void => {
            const value = getComputedStyle(document.documentElement)
                .getPropertyValue('--primary')
                .trim();

            if (value.length > 0) {
                setColor(value);
            }
        };

        readPrimary();

        const observer = new MutationObserver(readPrimary);
        observer.observe(document.documentElement, {
            attributes: true,
            attributeFilter: ['data-theme', 'class'],
        });

        return (): void => observer.disconnect();
    }, []);

    return <NextTopLoader color={color}/>;
}