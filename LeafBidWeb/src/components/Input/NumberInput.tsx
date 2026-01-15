import React from "react";
import {Form, InputGroup} from "react-bootstrap";

interface NumberInputProps {
    label: string;
    name: string;
    placeholder?: string;
    value?: number | string;
    step?: number | "any";
    min?: number;
    onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;

    // NEW:
    prefix?: string;
    postfix?: string;
}

const NumberInput: React.FC<NumberInputProps> = ({
                                                     label,
                                                     name,
                                                     placeholder,
                                                     value,
                                                     step = 1,
                                                     min = 0,
                                                     onChange,
                                                     prefix,
                                                     postfix
                                                 }) => {
    const control = (
        <Form.Control
            type="number"
            name={name}
            placeholder={placeholder}
            step={step}
            min={min}
            value={value}
            onChange={onChange}
        />
    );

    return (
        <Form.Group className="mb-3">
            <Form.Label>{label}</Form.Label>

            {prefix || postfix ? (
                <InputGroup>
                    {prefix && (
                        <InputGroup.Text id={`${name}-prefix`} style={{
                            backgroundColor: "var(--primary)",
                            borderColor: "var(--primary)",
                            color: "var(--background)"
                        }}>{prefix}</InputGroup.Text>
                    )}

                    {control}

                    {postfix && (
                        <InputGroup.Text id={`${name}-postfix`} style={{
                            backgroundColor: "var(--primary)",
                            borderColor: "var(--primary)",
                            color: "var(--background)"
                        }}>{postfix}</InputGroup.Text>
                    )}
                </InputGroup>
            ) : (
                control
            )}
        </Form.Group>
    );
};

export default NumberInput;
