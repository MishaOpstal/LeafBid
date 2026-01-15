"use client";

import React, {useEffect, useState} from "react";
import {Button, ButtonGroup} from "react-bootstrap";

interface SelectableButtonGroupProps {
    options: string[];
    name: string; // e.g. 'region', 'species'
    value?: string; // external controlled value
    onChange?: (name: string, value: string) => void;
    variantActive?: string;
    variantInactive?: string;
}

const SelectableButtonGroup: React.FC<SelectableButtonGroupProps> = ({
                                                                         options,
                                                                         name,
                                                                         value,
                                                                         onChange,
                                                                         variantActive = "primary",
                                                                         variantInactive = "secondary",
                                                                     }) => {
    const [selected, setSelected] = useState<string>(
        value ?? (options.length > 0 ? options[0] : "")
    );

    // Keep internal state synced when parent updates externally
    useEffect(() => {
        if (value !== undefined && value !== selected) {
            setSelected(value);
        }
    }, [selected, value]); // Only depend on value, not selected

    // Handle button click - only call onChange here, not in useEffect
    const handleSelect = (option: string) => {
        setSelected(option);
        // Call onChange immediately when the user clicks, not in useEffect
        if (onChange) {
            onChange(name, option);
        }
    };

    return (
        <ButtonGroup aria-label={`${name} options`} className="mb-3">
            {options.map((option) => (
                <Button
                    key={option}
                    variant={
                        selected === option ? variantActive : variantInactive
                    }
                    active={selected === option}
                    onClick={() => handleSelect(option)}
                    style={{
                        filter:
                            selected === option
                                ? "brightness(0.85)"
                                : "brightness(1)",
                        transition: "filter 0.15s ease-in-out",
                    }}
                >
                    {option}
                </Button>
            ))}
        </ButtonGroup>
    );
};

export default SelectableButtonGroup;