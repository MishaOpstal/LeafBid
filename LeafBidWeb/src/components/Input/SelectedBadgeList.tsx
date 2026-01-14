import React from "react";
import {Badge} from "react-bootstrap";
import s from "./SelectedBadgeList.module.css";
import {RegisteredProduct} from "@/types/Product/RegisteredProducts";

interface SelectedBadgeListProps {
    items: RegisteredProduct[];
    onRemove: (registeredProduct: RegisteredProduct) => void;
}

/**
 * Displays selected products as removable badges.
 */
const SelectedBadgeList: React.FC<SelectedBadgeListProps> = ({items, onRemove}) => {
    if (items.length === 0) return null;

    return (
        <div className={`mb-3 d-flex flex-wrap gap-2 ${s.badgeContainer}`}>
            {items.map((rp, i) => (
                <Badge
                    key={rp.id}
                    bg="secondary"
                    pill
                    className={s.selectedBadge}
                    title={`Click to remove ${rp.product!.name}`}
                    onClick={() => onRemove(rp)}
                >
                    {i + 1}. {rp.product!.name}
                </Badge>
            ))}
        </div>
    );
};

export default SelectedBadgeList;
