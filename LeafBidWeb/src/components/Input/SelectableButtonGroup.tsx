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

    // keep internal state synced when parent updates externally
    useEffect(() => {
        if (value !== undefined && value !== selected) {
            setSelected(value);
        }
    }, [selected, value]);

    // notify parent of selection changes
    useEffect(() => {
        if (onChange && selected) {
            onChange(name, selected);
        }
    }, [name, onChange, selected]);

    return (
        <ButtonGroup aria-label={`${name} options`} className="mb-3">
            {options.map((option) => (
                <Button
                    key={option}
                    variant={
                        selected === option ? variantActive : variantInactive
                    }
                    active={selected === option}
                    onClick={() => setSelected(option)}
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
