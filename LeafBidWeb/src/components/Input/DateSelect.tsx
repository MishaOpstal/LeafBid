import React, {useEffect, useState} from "react";
import {Form, Row} from "react-bootstrap";
import s from "./DateSelect.module.css";

interface DateSelectProps {
    label: string;
    placeholder?: string;
    onSelect: (date: string | null) => void;
    delay?: number;
    defaultValue?: string; // optional ISO datetime for initial state
    useTime?: boolean;
}

/**
 * A reusable Bootstrap date (+time) selector with debounced change events.
 */
const DateSelect: React.FC<DateSelectProps> = ({
                                                   label,
                                                   placeholder = "Select date...",
                                                   onSelect,
                                                   delay = 300,
                                                   defaultValue = "",
                                                   useTime = false,
                                               }) => {
    const [date, setDate] = useState<string>("");
    const [time, setTime] = useState<string>("");

    // Initialize if defaultValue (ISO string like "2025-11-11T14:30")
    useEffect(() => {
        if (defaultValue) {
            const [d, t] = defaultValue.split("T");
            setDate(d);
            if (useTime && t) setTime(t.slice(0, 5)); // keep HH:mm
        }
    }, [defaultValue, useTime]);

    // Debounce change callback
    useEffect(() => {
        const handler = setTimeout(() => {
            if (date) {
                const result = useTime && time ? `${date}T${time}` : date;
                onSelect(result);
            } else {
                onSelect(null);
            }
        }, delay);
        return () => clearTimeout(handler);
    }, [date, time, delay, onSelect, useTime]);

    return (
        <Form.Label className="mb-3">
            {label}
            <Row className={s.inputParents}>
                <Form.Control
                    type="date"
                    value={date}
                    placeholder={placeholder}
                    onChange={(e) => setDate(e.target.value)}
                    className={`${s.formControl} ${s.dateControl}`}
                />
                {useTime && (
                    <Form.Control
                        type="time"
                        value={time}
                        onChange={(e) => setTime(e.target.value)}
                        className={`${s.formControl} ${s.timeControl}`}
                    />
                )}
            </Row>
        </Form.Label>
    );
};

export default DateSelect;
