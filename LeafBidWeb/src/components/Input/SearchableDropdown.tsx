import React, {useEffect, useMemo, useRef, useState} from "react";
import {Dropdown} from "react-bootstrap";
import SearchBar from "./SearchBar";
import s from "./SearchableDropdown.module.css";

interface SearchableDropdownProps<T> {
    label?: string;
    items: T[];
    displayKey: keyof T;
    valueKey: keyof T;
    onSelect: (item: T) => void;
    placeholder?: string;

    value: string | number | null; // controlled selected value
}

function SearchableDropdown<T>({
                                   label = "Select an option",
                                   items,
                                   displayKey,
                                   valueKey,
                                   onSelect,
                                   placeholder = "Search...",
                                   value,
                               }: SearchableDropdownProps<T>) {
    const [query, setQuery] = useState<string>("");
    const [show, setShow] = useState<boolean>(false);
    const searchRef = useRef<HTMLInputElement>(null);

    const selectedItem = useMemo(() => {
        if (value === null || value === undefined) {
            return null;
        }

        return items.find((item) => String(item[valueKey]) === String(value)) ?? null;
    }, [items, value, valueKey]);

    const selectedText = selectedItem ? String(selectedItem[displayKey]) : null;

    const filteredItems = useMemo(() => {
        const q = query.toLowerCase();
        return q
            ? items.filter((item) => String(item[displayKey]).toLowerCase().includes(q))
            : items;
    }, [items, query, displayKey]);

    const handleSelect = (item: T) => {
        setQuery("");
        setShow(false);
        onSelect(item);
    };

    useEffect(() => {
        if (show && searchRef.current) {
            searchRef.current.focus();
        }
    }, [show]);

    return (
        <Dropdown
            show={show}
            onToggle={(isOpen) => setShow(isOpen)}
            className={s.dropdown}
        >
            <Dropdown.Toggle variant="outline-secondary" className={s.toggle}>
                {selectedText || label}
            </Dropdown.Toggle>

            <Dropdown.Menu className={s.menu}>
                <div className={s.searchContainer}>
                    <SearchBar
                        ref={searchRef}
                        placeholder={placeholder}
                        onSearch={setQuery}
                        delay={200}
                        value={query}
                        clearOnChange={false}
                    />
                </div>

                <div className={s.results}>
                    {filteredItems.length > 0 ? (
                        filteredItems.map((item) => (
                            <Dropdown.Item
                                key={String(item[valueKey])}
                                onClick={() => handleSelect(item)}
                            >
                                {String(item[displayKey])}
                            </Dropdown.Item>
                        ))
                    ) : (
                        <Dropdown.ItemText className="text-muted text-center">
                            No results found
                        </Dropdown.ItemText>
                    )}
                </div>
            </Dropdown.Menu>
        </Dropdown>
    );
}

export default SearchableDropdown;
