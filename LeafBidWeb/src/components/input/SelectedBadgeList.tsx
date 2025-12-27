import React from "react";
import { Badge } from "react-bootstrap";
import { Product } from "@/types/Product/Product";
import s from "./SelectedBadgeList.module.css";

interface SelectedBadgeListProps {
    items: Product[];
    onRemove: (product: Product) => void;
}

/**
 * Displays selected products as removable badges.
 */
const SelectedBadgeList: React.FC<SelectedBadgeListProps> = ({ items, onRemove }) => {
    if (items.length === 0) return null;

    return (
        <div className={`mb-3 d-flex flex-wrap gap-2 ${s.badgeContainer}`}>
            {items.map((p, i) => (
                <Badge
                    key={p.id}
                    bg="secondary"
                    pill
                    className={s.selectedBadge}
                    title={`Click to remove ${p.name}`}
                    onClick={() => onRemove(p)}
                >
                    {i + 1}. {p.name}
                </Badge>
            ))}
        </div>
    );
};

export default SelectedBadgeList;
