import React from "react";
import { Form } from "react-bootstrap";

interface FileInputProps {
    label: string;
    name: string;
    onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

const FileInput: React.FC<FileInputProps> = ({ label, name, onChange }) => {
    return (
        <Form.Label className="mb-3">
            {label}
            <Form.Control
                type="file"
                name={name}
                onChange={onChange}
            />
        </Form.Label>
    );
};

export default FileInput;
