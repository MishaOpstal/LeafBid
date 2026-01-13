import React, {forwardRef, useEffect, useState} from "react";
import {Form, InputGroup} from "react-bootstrap";
import {XCircleFill} from "react-bootstrap-icons";
import s from "./SearchBar.module.css";

interface SearchBarProps {
    placeholder?: string;
    onSearch: (query: string) => void;
    delay?: number;
    value?: string;
    clearOnChange?: boolean;
}

// ForwardRef allows parent to focus the Input
const SearchBar = forwardRef<HTMLInputElement, SearchBarProps>(
    (
        {
            placeholder = "Search...",
            onSearch,
            delay = 300,
            value: controlledValue,
        },
        ref
    ) => {
        const [value, setValue] = useState<string>(controlledValue ?? "");

        // keep internal value synced with parent
        useEffect(() => {
            if (controlledValue !== undefined) {
                setValue(controlledValue);
            }
        }, [controlledValue]);

        // debounce search calls
        useEffect(() => {
            const handler = setTimeout(() => {
                onSearch(value.trim());
            }, delay);
            return () => clearTimeout(handler);
        }, [value, delay, onSearch]);

        // clear function
        const handleClear = () => {
            setValue("");
            onSearch("");
        };

        return (
            <InputGroup className={`mb-3 ${s.inputGroup}`}>
                <Form.Control
                    ref={ref}
                    type="text"
                    value={value}
                    placeholder={placeholder}
                    onChange={(e) => setValue(e.target.value)}
                    className={s.formControl}
                />
                {value && (
                    <InputGroup.Text
                        onClick={handleClear}
                        className={s.clearButton}
                        title="Clear search"
                    >
                        <XCircleFill size={16}/>
                    </InputGroup.Text>
                )}
            </InputGroup>
        );
    }
);

SearchBar.displayName = "SearchBar";
export default SearchBar;
