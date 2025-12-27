import React, { useMemo, useState, useEffect } from "react";
import { ListGroup, Form, Badge, Button, Spinner } from "react-bootstrap";
import SearchBar from "./SearchBar";
import SelectedBadgeList from "./SelectedBadgeList";
import { Product } from "@/types/Product/Product";
import s from "./OrderedMultiSelect.module.css";

interface OrderedMultiSelectProps {
    items?: Product[];
    value?: Product[]; // controlled selection
    onChange?: (selected: Product[]) => void;
    pageSize?: number;
    endpoint?: string;
    showBadges?: boolean;
}

/**
 * A stable, fully controlled, paginated and searchable multi-select list.
 * Works with both local (dummy) data and remote API data.
 */
const OrderedMultiSelect: React.FC<OrderedMultiSelectProps> = ({
                                                                   items = [],
                                                                   value = [],
                                                                   onChange,
                                                                   pageSize = 10,
                                                                   endpoint,
                                                                   showBadges = true,
                                                               }) => {
    const [query, setQuery] = useState("");
    const [page, setPage] = useState(1);
    const [remoteItems, setRemoteItems] = useState<Product[]>([]);
    const [totalPages, setTotalPages] = useState(1);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Fetch remote data if endpoint is provided
    useEffect(() => {
        if (!endpoint) return;

        const controller = new AbortController();

        const fetchData = async () => {
            setLoading(true);
            setError(null);

            try {
                const url = new URL(endpoint);
                url.searchParams.set("page", String(page));
                url.searchParams.set("limit", String(pageSize));
                if (query.trim()) url.searchParams.set("q", query);

                const res = await fetch(url.toString(), { signal: controller.signal });
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const data = await res.json();

                setRemoteItems(data.data ?? data);
                setTotalPages(data.totalPages ?? 1);
            } catch (err) {
                if (err instanceof DOMException && err.name === "AbortError") return;
                setError("Failed to load products.");
            } finally {
                setLoading(false);
            }
        };

        fetchData();
        return () => controller.abort();
    }, [endpoint, page, query, pageSize]);

    // Compute displayed items
    const filteredItems = useMemo(() => {
        if (endpoint) return remoteItems;

        const q = query.toLowerCase();
        const filtered = q
            ? items.filter((p) => p.name.toLowerCase().includes(q))
            : items;

        setTotalPages(Math.max(1, Math.ceil(filtered.length / pageSize)));
        const start = (page - 1) * pageSize;
        return filtered.slice(start, start + pageSize);
    }, [items, query, page, pageSize, endpoint, remoteItems]);

    // Always render exactly `pageSize` rows (fill with placeholders)
    const displayItems = useMemo(() => {
        const filled = [...filteredItems];
        const remaining = pageSize - filled.length;
        for (let i = 0; i < remaining; i++) filled.push(null as never);
        return filled;
    }, [filteredItems, pageSize]);

    const handleToggle = (product: Product) => {
        if (!onChange) return;
        const exists = value.some((p) => p.id === product.id);
        const updated = exists
            ? value.filter((p) => p.id !== product.id)
            : [...value, product];
        onChange(updated);
    };

    const nextPage = () => setPage((p) => Math.min(p + 1, totalPages));
    const prevPage = () => setPage((p) => Math.max(p - 1, 1));

    // Reset to first page when query changes
    useEffect(() => setPage(1), [query]);

    return (
        <div className="p-3 border rounded bg-light">
            {/* Search bar */}
            <SearchBar placeholder="Search products..." onSearch={setQuery} delay={300} />

            {/* Optional badges */}
            {showBadges && value.length > 0 && (
                <SelectedBadgeList items={value} onRemove={handleToggle} />
            )}

            {/* Product list */}
            <ListGroup className={s.listGroup}>
                {loading ? (
                    <div className="text-center py-4">
                        <Spinner animation="border" size="sm" /> Loading...
                    </div>
                ) : error ? (
                    <ListGroup.Item className="text-danger text-center">{error}</ListGroup.Item>
                ) : (
                    displayItems.map((product, idx) =>
                        product ? (
                            <ListGroup.Item
                                key={product.id}
                                action
                                onClick={(e) => {
                                    e.preventDefault();
                                    handleToggle(product);
                                }}
                                className={`d-flex align-items-center justify-content-between ${
                                    value.some((p) => p.id === product.id)
                                        ? "active"
                                        : ""
                                } ${s.listItem}`}
                            >
                                <div className={s.productRow}>
                                    <strong className={s.productName}>{product.name}</strong>
                                    <span className={s.quantity}>
                                        Qty: {product.stock ?? "N/A"}
                                    </span>
                                </div>

                                {value.some((p) => p.id === product.id) && (
                                    <Badge bg="secondary" pill className={s.badge}>
                                        {value.findIndex(
                                            (p) => p.id === product.id
                                        ) + 1}
                                    </Badge>
                                )}
                            </ListGroup.Item>
                        ) : (
                            <ListGroup.Item
                                key={`placeholder-${idx}`}
                                className={`text-muted ${s.placeholderItem}`}
                            >
                                &nbsp;
                            </ListGroup.Item>
                        )
                    )
                )}
            </ListGroup>

            {/* Pagination */}
            {totalPages > 1 && (
                <div className="d-flex justify-content-between align-items-center mt-3">
                    <Button
                        variant="outline-secondary"
                        size="sm"
                        onClick={prevPage}
                        disabled={page === 1 || loading}
                    >
                        ← Previous
                    </Button>
                    <span className="text-muted small">
                        Page {page} of {totalPages}
                    </span>
                    <Button
                        variant="outline-secondary"
                        size="sm"
                        onClick={nextPage}
                        disabled={page === totalPages || loading}
                    >
                        Next →
                    </Button>
                </div>
            )}
        </div>
    );
};

export default OrderedMultiSelect;
