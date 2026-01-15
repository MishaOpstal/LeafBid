'use client';

import React from 'react';
import {Button, Card} from 'react-bootstrap';
import Placeholder from "react-bootstrap/Placeholder";
import {parsePrice} from "@/types/Product/RegisteredProducts";
import s from "./DashboardPanel.module.css";

type DashboardPanelProps = {
    title?: string;
    kloklocatie?: string;
    imageSrc?: string;
    auctionStatus?: string;
    huidigePrijs?: number;
    aankomendProductNaam?: string;
    aankomendProductStartprijs?: number;
    loading?: boolean;
    compact?: boolean;
    isLive?: boolean;
    isFinished?: boolean;
    activeClockLocations?: string[];
    onStartAuction?: () => void;
    onStopAuction?: () => void;
    onError?: (msg: string) => void;
};
const DashboardPanel: React.FC<DashboardPanelProps> = ({
                                                           title,
                                                           kloklocatie,
                                                           imageSrc,
                                                           auctionStatus,
                                                           huidigePrijs,
                                                           aankomendProductNaam,
                                                           aankomendProductStartprijs,
                                                           loading = false,
                                                           compact = false,
                                                           isLive,
                                                           isFinished,
                                                           activeClockLocations,
                                                           onStartAuction,
                                                           onStopAuction,
                                                           onError
                                                       }) => {
// compacte kaart
    if (compact) {
        return (
            <Card className={`d-flex flex-row ${s.card}`}>
                {/* Image on the left, small width */}
                <Card.Img
                    alt={`Foto van ${title}`}
                    className={`${s.image}`}
                    variant="left"
                    src={imageSrc || "/images/grey.png"}
                    style={{width: "100px", height: "100px", objectFit: "cover"}}
                />


                <Card.Body className="d-flex justify-content-between align-items-center py-2">
                    {/* Left side: title + time inline */}
                    <div className="d-flex align-items-center gap-2">
                        {loading ? (
                            <>
                                {/*loading compact card*/}
                                <div style={{width: "120px"}}>
                                    <Placeholder as={Card.Title} animation="glow">
                                        <Placeholder xs={8}/>
                                    </Placeholder>
                                    <Placeholder as="small" animation="glow">
                                        <Placeholder xs={6}/>
                                    </Placeholder>
                                </div>

                            </>
                        ) : (
                            // standard compact card
                            <>
                                <Card.Title className="mb-0">{kloklocatie}</Card.Title>
                                <small className={s.statusText}>{auctionStatus}</small>
                            </>
                        )}
                    </div>

                    <div className="d-flex gap-2">
                        {loading ? (
                            <Placeholder.Button variant="secondary" xs={2}/>
                        ) : (
                            <>
                                {!isFinished && (
                                    !isLive ? (
                                        <Button
                                            variant="success"
                                            size="sm"
                                            onClick={() => {
                                                const alreadyActive = activeClockLocations?.includes(kloklocatie ?? "");

                                                if (alreadyActive) {
                                                    onError?.(`Er draait al een veiling op ${kloklocatie}.`);
                                                    return;
                                                }

                                                onStartAuction?.();
                                            }}
                                        >
                                            Start
                                        </Button>
                                    ) : (
                                        <Button
                                            variant="danger"
                                            size="sm"
                                            onClick={() => onStopAuction?.()}
                                            className={"btn-danger"}
                                        >
                                            Stop
                                        </Button>
                                    )
                                )}
                            </>
                        )}
                    </div>
                </Card.Body>
            </Card>
        );
    }

    // Standaard kaart
    return (
        <Card className={`d-flex flex-column flex-md-row ${s.card}`}>
            <Card.Img
                alt={`Foto van ${title}`}
                className={`${s.image}`}
                variant="left"
                src={imageSrc || "/images/grey.png"}
            />
            <Card.Body className="w-100">
                <div className="d-flex flex-column flex-md-row gap-3">
                    {/* First block */}
                    <div className="flex-fill w-100 w-md-75">
                        {loading ? (
                            <>
                                <Placeholder as={Card.Title} animation="glow">
                                    <Placeholder xs={6}/>
                                </Placeholder>
                                <Placeholder as={Card.Text} animation="glow">
                                    <Placeholder xs={7}/> <Placeholder xs={4}/> <Placeholder xs={5}/>
                                </Placeholder>
                            </>
                        ) : (
                            <>
                                <Card.Title>Kloklocatie: {kloklocatie}</Card.Title>
                                <Card.Text>
                                    <span>Huidig product: {title}</span><br/>
                                    <span>Huidige prijs: {parsePrice(huidigePrijs ?? 0)}</span><br/>
                                    <span>Status: {auctionStatus}</span><br/>
                                </Card.Text>

                            </>
                        )}
                    </div>

                    {/*loading card*/}
                    <div className="flex-fill w-100 w-md-25">
                        {loading ? (
                            <>
                                <Placeholder as={Card.Title} animation="glow">
                                    <Placeholder xs={5}/>
                                </Placeholder>
                                <Placeholder as={Card.Text} animation="glow">
                                    <Placeholder xs={6}/> <Placeholder xs={3}/>
                                </Placeholder>
                            </>
                        ) : (
                            <>
                                <Card.Title>Aankomende producten</Card.Title>
                                <Card.Text>
                                    <span className="mb-0 d-block">{aankomendProductNaam}</span>
                                    <span className="mb-0 d-block"
                                          style={{fontSize: "0.7rem"}}>Startprijs: {parsePrice(aankomendProductStartprijs ?? 0)}</span>
                                </Card.Text>
                            </>
                        )}
                    </div>
                </div>
            </Card.Body>
        </Card>
    );
};


export default DashboardPanel;
