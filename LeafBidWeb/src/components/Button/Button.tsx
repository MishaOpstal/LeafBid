'use client';

import React from 'react';
import { useRouter } from 'nextjs-toploader/app';
import s from './Button.module.css';

type KnopProps = {
    label: string;
    to: string;
};

const Button: React.FC<KnopProps> = ({ label, to }) => {
    const router = useRouter();

    const handleClick = () => {
        router.push(to);
    };

    return (
        <button className={s.button} onClick={handleClick}>
            {label}
        </button>
    );
};

export default Button;
