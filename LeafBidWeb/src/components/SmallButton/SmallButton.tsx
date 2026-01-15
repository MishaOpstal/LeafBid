'use client';

import React from "react";
import {Button} from "react-bootstrap";
import {Pencil, Trash} from "react-bootstrap-icons";
import styles from "./SmallButton.module.css";

type ActionButtonsProps = {
    onDelete?: () => void;
    onUpdate?: () => void;
};

const ActionButtons: React.FC<ActionButtonsProps> = ({onDelete, onUpdate}) => {

    return (
        <div className="d-flex gap-2">
            <Button
                aria-label="verwijder veiling"
                className={styles.transparentButton}
                variant="light"
                size="sm"
                onClick={onDelete}
            >
                <Trash style={{color: "var(--primary-background)"}}/>
            </Button>
            <Button
                aria-label="wijzig veiling"
                className={styles.transparentButton}
                variant="light"
                size="sm"
                onClick={onUpdate}
            >
                <Pencil style={{color: "var(--primary-background)"}}/>
            </Button>
        </div>
    );
};

export default ActionButtons;
