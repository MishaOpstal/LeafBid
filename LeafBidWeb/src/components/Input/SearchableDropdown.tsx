import React, { useState, useMemo, useRef, useEffect } from "react";
import { Dropdown } from "react-bootstrap";
import SearchBar from "./SearchBar";
import s from "./SearchableDropdown.module.css";

interface SearchableDropdownProps<T> {
    label?: string;
    items: T[];
    displayKey: keyof T;
    valueKey: keyof T;
    onSelect: (item: T) => void;
    placeholder?: string;
}

function SearchableDropdown<T>({
                                   label = "Select an option",
                                   items,
                                   displayKey,
                                   valueKey,
                                   onSelect,
                                   placeholder = "Search...",
                               }: SearchableDropdownProps<T>) {
    const [query, setQuery] = useState<string>("");
    const [selectedLabel, setSelectedLabel] = useState<string | null>(null);
    const [show, setShow] = useState<boolean>(false);
    const searchRef = useRef<HTMLInputElement>(null);

    const filteredItems = useMemo(() => {
        const q = query.toLowerCase();
        return q
            ? items.filter((item) =>
                String(item[displayKey]).toLowerCase().includes(q)
            )
            : items;
    }, [items, query, displayKey]);

    const handleSelect = (item: T) => {
        setSelectedLabel(String(item[displayKey]));
        setQuery(""); // 🧹 clear the search
        setShow(false);
        onSelect(item);
    };

    // Focus search when the dropdown opens
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
                {selectedLabel || label}
            </Dropdown.Toggle>

            <Dropdown.Menu className={s.menu}>
                {/* Search Input */}
                <div className={s.searchContainer}>
                    <SearchBar
                        ref={searchRef}
                        placeholder={placeholder}
                        onSearch={setQuery}
                        delay={200}
                        value={query} // pass controlled value
                        clearOnChange={false}
                    />
                </div>

                {/* Filtered results */}
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
