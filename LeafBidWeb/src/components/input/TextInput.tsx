import React from "react";
import {Form, InputGroup} from "react-bootstrap";

interface TextInputProps {
    label: string;
    name: string;
    placeholder?: string;
    value?: string;
    onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
    secret?: boolean;

    // NEW:
    prefix?: string;
    postfix?: string;
}

const TextInput: React.FC<TextInputProps> = ({
                                                 label,
                                                 name,
                                                 placeholder,
                                                 value,
                                                 onChange,
                                                 secret,
                                                 prefix,
                                                 postfix
                                             }) => {

    const control = (
        <Form.Control
            type={secret ? "password" : "text"}
            name={name}
            placeholder={placeholder}
            value={value}
            onChange={onChange}
        />
    );

    return (
        <Form.Group className="mb-3">
            <Form.Label>{label}</Form.Label>

            {prefix || postfix ? (
                <InputGroup className="mb-3">
                    {prefix && (
                        <InputGroup.Text id={`${name}-prefix`}>{prefix}</InputGroup.Text>
                    )}

                    {control}

                    {postfix && (
                        <InputGroup.Text id={`${name}-postfix`}>{postfix}</InputGroup.Text>
                    )}
                </InputGroup>
            ) : (
                control
            )}
        </Form.Group>
    );
};

export default TextInput;
