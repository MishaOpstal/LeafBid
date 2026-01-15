import s from '@/components/AuctionedProduct/AuctionedProduct.module.css';
import {parsePrice, RegisteredProduct} from "@/types/Product/RegisteredProducts";
import {Image} from "react-bootstrap";
import Button from "@/components/Input/Button";
import NumberInput from "@/components/Input/NumberInput";
import {resolveImageSrc} from "@/utils/Image";
import {parseDate} from "@/utils/Time";
import React, {useEffect, useState} from "react";

export default function AuctionedProduct({registeredProduct, currentPricePerUnit, onBuy, isPaused, isLive}: {
    registeredProduct: RegisteredProduct,
    currentPricePerUnit: number,
    onBuy: (amount: number) => Promise<void>,
    isPaused?: boolean,
    isLive?: boolean
}) {
    const [amount, setAmount] = useState<number>(1);
    const [isBuying, setIsBuying] = useState(false);

    useEffect(() => {
        setAmount(1);
    }, [registeredProduct]);

    const maxStock = Math.max(0, registeredProduct.stock ?? 0);

    const clamp = (value: number): number => {
        if (maxStock <= 0) return 0;
        if (value < 1) return 1;
        if (value > maxStock) return maxStock;
        return value;
    };

    const handleAmountChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const raw = e.target.value;

        if (raw === "") {
            setAmount(0);
            return;
        }

        const parsed = Number(raw);
        if (Number.isNaN(parsed)) return;

        setAmount(clamp(parsed));
    };

    const canBuy = maxStock > 0 && amount >= 1 && amount <= maxStock && !isBuying && !isPaused;

    const handleBuy = async () => {
        if (!canBuy) return;
        setIsBuying(true);
        try {
            await onBuy(amount);
        } finally {
            setIsBuying(false);
        }
    };

    return (
        <div className={`d-flex flex-column ${s.wrapper}`}>
            <div className="d-flex flex-row gap-4">
                <Image
                    src={resolveImageSrc(registeredProduct.product!.picture)}
                    alt={registeredProduct.product!.name}
                    className={`mb-3 ${s.image}`}
                />
                <div className={`d-flex flex-row gap-1 ${s.infoBox}`}>
                    <p>{registeredProduct.product!.description}</p>
                </div>
            </div>

            <div className={`d-flex flex-column gap-3 p-3 ${s.textContainer}`}>
                <h2>{registeredProduct.product!.name}</h2>
                <p>Aantal: {registeredProduct.stock}</p>
                <p>Geoogst: {parseDate(registeredProduct.harvestedAt ?? "")}</p>
                <p>Leverancier: {registeredProduct.company!.name}</p>
                <p>Regio Oorsprong: {registeredProduct.region}</p>
            </div>

            <div className="d-flex flex-column gap-2">
                <NumberInput
                    label="Aantal te kopen"
                    name="amount"
                    value={amount === 0 ? "" : amount}
                    step={1}
                    onChange={handleAmountChange}
                    postfix={maxStock > 0 ? `/ ${maxStock}` : undefined}
                />

                <div>
                    <Button
                        label={isBuying ? "Bezig met kopen..." : isPaused ? (isLive ? "Veiling gepauzeerd" : "Wachten op start") : `Koop voor ${parsePrice(currentPricePerUnit * amount)}`}
                        variant="primary"
                        type="button"
                        className={s.button}
                        disabled={!canBuy}
                        onClick={handleBuy}
                    />
                </div>
            </div>
        </div>
    );
}
