import React from "react";
import {Form} from "react-bootstrap";

interface TextAreaInputProps {
    label: string;
    name: string;
    placeholder?: string;
    rows?: number;
    value?: string;
    onChange?: (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => void;
}

const TextAreaInput: React.FC<TextAreaInputProps> = ({
                                                         label,
                                                         name,
                                                         placeholder,
                                                         rows = 3,
                                                         value,
                                                         onChange,
                                                     }) => {
    return (
        <Form.Label className="mb-3">
            {label}
            <Form.Control
                as="textarea"
                name={name}
                rows={rows}
                placeholder={placeholder}
                value={value}
                onChange={onChange}
            />
        </Form.Label>
    );
};

export default TextAreaInput;
