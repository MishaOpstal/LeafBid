import React from "react";
import {Button as BsButton} from "react-bootstrap";

interface ButtonProps {
    label: string;
    variant?: string;
    type?: "button" | "submit" | "reset";
    disabled?: boolean;
    className?: string;
    onClick?: (e: React.MouseEvent<HTMLButtonElement>) => void;
}

/**
 * A flexible Bootstrap-based Button component.
 * Defaults to a non-submitting Button (type="Button").
 */
const Button: React.FC<ButtonProps> = ({
                                           label,
                                           variant = "primary",
                                           type = "button",
                                           disabled = false,
                                           className = "",
                                           onClick,
                                       }) => {
    return (
        <BsButton
            type={type}
            variant={variant}
            disabled={disabled}
            onClick={onClick}
            className={className}
        >
            {label}
        </BsButton>
    );
};

export default Button;
