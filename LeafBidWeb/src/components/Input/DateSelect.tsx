import React, {useEffect, useMemo, useState} from "react";
import {Form, Row} from "react-bootstrap";
import s from "./DateSelect.module.css";

interface DateSelectProps {
    label: string;
    placeholder?: string;
    onSelect: (date: string | null) => void;
    delay?: number;
    defaultValue?: string;
    useTime?: boolean;
    disallowPast?: boolean;
    disallowFuture?: boolean;
}

const getTodayISO = (d: Date): string => {
    return d.toISOString().slice(0, 10);
};

const getHHMM = (d: Date): string => {
    return d.toTimeString().slice(0, 5);
};

const getNextMinuteHHMM = (from: Date): string => {
    const d: Date = new Date(from);
    d.setSeconds(0, 0);
    d.setMinutes(d.getMinutes() + 5);
    return getHHMM(d);
};

const isTimeBeforeOrEqual = (a: string, b: string): boolean => {
    return a <= b;
};

const isTimeAfterOrEqual = (a: string, b: string): boolean => {
    return a >= b;
};

const parseLocalDateOrDateTime = (dateISO: string, timeHHMM?: string): Date => {
    // dateISO: YYYY-MM-DD, timeHHMM: HH:mm (optional)
    const year: number = Number(dateISO.slice(0, 4));
    const month: number = Number(dateISO.slice(5, 7));
    const day: number = Number(dateISO.slice(8, 10));

    if (!timeHHMM) {
        return new Date(year, month - 1, day, 0, 0, 0, 0);
    }

    const hour: number = Number(timeHHMM.slice(0, 2));
    const minute: number = Number(timeHHMM.slice(3, 5));

    return new Date(year, month - 1, day, hour, minute, 0, 0);
};

const DateSelect: React.FC<DateSelectProps> = ({
                                                   label,
                                                   placeholder = "Select date...",
                                                   onSelect,
                                                   delay = 300,
                                                   defaultValue = "",
                                                   useTime = false,
                                                   disallowPast = false,
                                                   disallowFuture = false,
                                               }) => {
    const [date, setDate] = useState<string>("");
    const [time, setTime] = useState<string>("");

    // A ticking "now" so min/max stays fresh.
    const [nowTick, setNowTick] = useState<number>(() => Date.now());

    const nowDate: Date = useMemo(() => new Date(nowTick), [nowTick]);
    const todayISO: string = useMemo(() => getTodayISO(nowDate), [nowDate]);

    const dateMin: string | undefined = disallowPast ? todayISO : undefined;
    const dateMax: string | undefined = disallowFuture ? todayISO : undefined;

    const timeMin: string | undefined =
        disallowPast && useTime && date === todayISO
            ? getNextMinuteHHMM(nowDate)
            : undefined;

    const timeMax: string | undefined =
        disallowFuture && useTime && date === todayISO
            ? getHHMM(nowDate)
            : undefined;

    // Initialize if defaultValue (ISO string like "2025-11-11T14:30")
    useEffect(() => {
        if (defaultValue) {
            const [d, t] = defaultValue.split("T");
            setDate(d);

            if (useTime && t) {
                setTime(t.slice(0, 5));
            }
        }
    }, [defaultValue, useTime]);

    // Keep "now" moving while we care about min/max time (today + (disallowPast/future) + useTime).
    useEffect(() => {
        if (!useTime) {
            return;
        }

        if (!disallowPast && !disallowFuture) {
            return;
        }

        const interval: number = window.setInterval(() => {
            setNowTick(Date.now());
        }, 10_000);

        return () => {
            window.clearInterval(interval);
        };
    }, [disallowPast, disallowFuture, useTime]);

    // Clamp time if it becomes invalid (e.g. user left the page open and min/max moved).
    useEffect(() => {
        if (!useTime) {
            return;
        }

        if (!date || date !== todayISO) {
            return;
        }

        if (disallowPast && timeMin && time && isTimeBeforeOrEqual(time, timeMin)) {
            setTime(timeMin);
            return;
        }

        if (disallowFuture && timeMax && time && isTimeAfterOrEqual(time, timeMax)) {
            setTime(timeMax);
        }
    }, [date, time, timeMin, timeMax, todayISO, disallowPast, disallowFuture, useTime]);

    const handleDateChange = (newDate: string) => {
        setDate(newDate);

        if (!useTime) {
            return;
        }

        if (newDate === todayISO) {
            if (disallowPast && timeMin && time && isTimeBeforeOrEqual(time, timeMin)) {
                setTime(timeMin);
            }

            if (disallowFuture && timeMax && time && isTimeAfterOrEqual(time, timeMax)) {
                setTime(timeMax);
            }
        }
    };

    const handleTimeChange = (newTime: string) => {
        if (!useTime || !date) {
            setTime(newTime);
            return;
        }

        if (date === todayISO) {
            if (disallowPast && timeMin) {
                // Compare against the actual minimum allowed time (next minute), not current HH:mm.
                if (isTimeBeforeOrEqual(newTime, timeMin)) {
                    setTime(timeMin);
                    return;
                }
            }

            if (disallowFuture && timeMax) {
                if (isTimeAfterOrEqual(newTime, timeMax)) {
                    setTime(timeMax);
                    return;
                }
            }
        }

        setTime(newTime);
    };

    // Debounce change callback
    useEffect(() => {
        const handler: number = window.setTimeout(() => {
            if (!date) {
                onSelect(null);
                return;
            }

            const result: string = useTime && time ? `${date}T${time}` : date;

            if (disallowPast || disallowFuture) {
                const now: Date = new Date();
                const selected: Date = useTime && time
                    ? parseLocalDateOrDateTime(date, time)
                    : parseLocalDateOrDateTime(date);

                if (disallowPast) {
                    if (selected <= now) {
                        return;
                    }
                }

                if (disallowFuture) {
                    if (selected > now) {
                        return;
                    }
                }
            }

            onSelect(result);
        }, delay);

        return () => {
            window.clearTimeout(handler);
        };
    }, [date, time, delay, onSelect, useTime, disallowPast, disallowFuture]);

    return (
        <Form.Label className="mb-3">
            {label}
            <Row className={s.inputParents}>
                <Form.Control
                    type="date"
                    value={date}
                    placeholder={placeholder}
                    min={dateMin}
                    max={dateMax}
                    onChange={(e) => handleDateChange(e.target.value)}
                    className={`${s.formControl} ${s.dateControl}`}
                />

                {useTime && (
                    <Form.Control
                        type="time"
                        value={time}
                        min={timeMin}
                        max={timeMax}
                        step={60}
                        onChange={(e) => handleTimeChange(e.target.value)}
                        className={`${s.formControl} ${s.timeControl}`}
                    />
                )}
            </Row>
        </Form.Label>
    );
};

export default DateSelect;
