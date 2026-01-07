"use client";

import s from "@/app/layouts/toevoegen/page.module.css";
import ToevoegenLayout from "@/app/layouts/toevoegen/layout";

import Form from "react-bootstrap/Form";
import Button from "@/components/input/Button";
import SearchableDropdown from "@/components/input/SearchableDropdown";
import DateSelect from "@/components/input/DateSelect";
import React, { useEffect, useState } from "react";
import { isUserInRole } from "@/app/auth/utils/isUserInRole";

import type {User} from "@/types/User/User";

interface UserMeResponse {
    loggedIn: boolean;
    userData?: (User & {
        companyId?: number | string;
        company?: { id?: number | string; companyId?: number | string } | null;
    }) | null;
}

// Optional access gate like nieuw/page.tsx
if (!isUserInRole("Provider") && !isUserInRole("Admin")) {
    window.location.href = "/";
}

// Utility: try to parse JSON safely
const safeParseJSON = async (res: Response) => {
    try {
        const text = await res.text();
        try {
            return { json: JSON.parse(text), raw: text };
        } catch {
            return { json: null, raw: text };
        }
    } catch {
        return { json: null, raw: null };
    }
};

// Utility: normalize various ASP.NET error shapes to a common structure
interface NormalizedServerError {
    status: number;
    title?: string;
    detail?: string;
    message?: string;
    errors?: Record<string, string[]>; // ModelState-like { field: [messages] }
    raw?: string | null;
}

const parseErrorResponse = async (res: Response): Promise<NormalizedServerError> => {
    const ct = res.headers.get("content-type") || "";
    // Try to read body as text/JSON either way
    const { json, raw } = await safeParseJSON(res);

    // ASP.NET Core default problem details
    // {
    //   type, title, status, detail, instance,
    //   errors?: { [key: string]: string[] } // for validation
    // }
    if (ct.includes("application/problem+json") || (json && (json.title || json.errors))) {
        return {
            status: res.status,
            title: json?.title,
            detail: json?.detail,
            message: json?.message,
            errors: json?.errors ?? undefined,
            raw,
        };
    }

    // Custom error envelope with message
    if (json && (json.message || json.error || json.errorMessage)) {
        return {
            status: res.status,
            message: json.message || json.error || json.errorMessage,
            raw,
        };
    }

    // Plain text/HTML fallback
    return {
        status: res.status,
        message: `Server returned ${res.status}`,
        detail: raw ?? undefined,
        raw,
    };
};

// Minimal product type to populate the dropdown
type ProductItem = { id: number; name: string };


// Backend payload shape you provided (without productId, because it goes in the route)
interface SaleProductPayloadBody {
    id?: number;
    companyId: number;
    userId: string;
    minPrice: number;
    maxPrice?: number | null;
    stock: number;
    region: string;
    harvestedAt: string; // ISO string (DateTime)
    potSize?: number | null;
    stemLength?: number | null;
}

export default function ProductSaleCreatePage() {
    const [products, setProducts] = useState<ProductItem[]>([]);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [message, setMessage] = useState("");
    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isLoadingUser, setIsLoadingUser] = useState(true);

    // Fetched identities
    const [userId, setUserId] = useState<string>("");
    const [companyId, setCompanyId] = useState<number | null>(null);

    const [formData, setFormData] = useState({
        productId: 0,
        minPrice: 0,
        maxPrice: undefined as number | undefined,
        stock: 0,
        region: "",
        harvestedAt: "",
        potSize: undefined as number | undefined,
        stemLength: undefined as number | undefined,
    });

    // Helper to extract company id from various possible shapes on userData
    const extractCompanyId = (userData: UserMeResponse["userData"]): number | null => {
        if (!userData) return null;

        const candidates: Array<number | string | undefined> = [
            userData.companyId,
            userData.company?.companyId,
            userData.company?.id,
        ];

        for (const c of candidates) {
            if (typeof c === "number" && Number.isFinite(c)) return c;
            if (typeof c === "string") {
                const n = Number(c);
                if (Number.isFinite(n)) return n;
            }
        }
        return null;
    };

    // Fetch current user (and company) on mount
    useEffect(() => {
        let cancelled = false;
        (async () => {
            try {
                const res = await fetch("http://localhost:5001/api/v2/User/me", {
                    method: "GET",
                    credentials: "include",
                });
                if (!res.ok) throw new Error("Failed to fetch current user");
                const me: UserMeResponse = await res.json();

                if (!me?.loggedIn) {
                    if (!cancelled) {
                        setErrors((prev) => ({ ...prev, auth: "Je bent niet ingelogd." }));
                    }
                    return;
                }

                const id = me.userData?.id ?? "";
                const compId = extractCompanyId(me?.userData);

                if (!id) {
                    if (!cancelled) setErrors((prev) => ({ ...prev, userId: "Gebruikers-ID niet gevonden." }));
                } else if (!cancelled) {
                    setUserId(String(id));
                }

                // Debug: Force companyId to 1
                if (!cancelled) {
                    setCompanyId(1);
                }
                /* Commented out original logic for debugging
                if (compId == null) {
                    if (!cancelled) setErrors((prev) => ({ ...prev, companyId: "Gebruiker heeft geen bedrijf." }));
                } else if (!cancelled) {
                    setCompanyId(compId);
                }
                */

            } catch (e) {
                console.error(e);
                if (!cancelled) setErrors((prev) => ({ ...prev, auth: "Kon huidige gebruiker niet laden." }));
            } finally {
                if (!cancelled) setIsLoadingUser(false);
            }
        })();
        return () => {
            cancelled = true;
        };
    }, []);

    // Load available products
    useEffect(() => {
        (async () => {
            try {
                const res = await fetch("http://localhost:5001/api/v2/Product/available", {
                    method: "GET",
                    credentials: "include",
                });
                if (!res.ok) throw new Error("Failed to fetch products");
                const data = await res.json();
                const arr: unknown = data;
                const safeArr = Array.isArray(arr) ? arr : [];
                const mapped: ProductItem[] = safeArr.map((p) => {
                    const obj = (p && typeof p === "object") ? (p as Record<string, unknown>) : {};
                    const idRaw = obj["id"] ?? obj["productId"] ?? obj["Id"];
                    const nameRaw = obj["name"] ?? obj["Name"];
                    return {
                        id: typeof idRaw === "number" ? idRaw : Number(idRaw ?? NaN),
                        name: typeof nameRaw === "string" ? nameRaw : String(nameRaw ?? ""),
                    };
                });
                setProducts(mapped);
            } catch (err) {
                console.error("Failed to fetch products:", err);
            }
        })();
    }, []);

    const validate = (): boolean => {
        const e: Record<string, string> = {};
        if (!formData.productId || formData.productId <= 0) e.productId = "Product is verplicht.";
        if (!companyId) e.companyId = "CompanyId is verplicht.";
        if (!userId) e.userId = "UserId is verplicht.";
        if (formData.minPrice == null || formData.minPrice < 0) e.minPrice = "Min. prijs is verplicht en moet ≥ 0 zijn.";
        if (formData.stock == null || formData.stock < 0) e.stock = "Voorraad is verplicht en moet ≥ 0 zijn.";
        if (!formData.region) e.region = "Regio is verplicht.";
        if (!formData.harvestedAt) e.harvestedAt = "Oogst-datum/tijd is verplicht.";
        setErrors(e);
        return Object.keys(e).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setMessage("");
        setErrors({});
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            const productId = Number(formData.productId);
            if (!productId || Number.isNaN(productId)) throw new Error("Ongeldige productId.");

            // Normalize fields
            const region = (formData.region || "").trim();

            // If your DateSelect already gives ISO string we keep it; otherwise ensure ISO with toISOString()
            const harvestedAtISO = formData.harvestedAt
                ? new Date(formData.harvestedAt).toISOString()
                : "";

            const body: SaleProductPayloadBody = {
                companyId: companyId!,
                userId: userId,
                minPrice: Number(formData.minPrice),
                maxPrice: formData.maxPrice != null ? Number(formData.maxPrice) : null,
                stock: Number(formData.stock),
                region,
                harvestedAt: harvestedAtISO,
                potSize: formData.potSize != null ? Number(formData.potSize) : null,
                stemLength: formData.stemLength != null ? Number(formData.stemLength) : null,
            };

            const url = `http://localhost:5001/api/v2/AuctionSaleProduct/registeredCreate/${encodeURIComponent(productId)}`;

            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    Accept: "application/json, application/problem+json;q=0.9, */*;q=0.1",
                },
                credentials: "include",
                body: JSON.stringify(body),
            });

            if (!response.ok) {
                const normalized = await parseErrorResponse(response);

                // Map ModelState-like errors to your field names where possible
                const fieldErrorMap: Record<string, string> = {};
                if (normalized.errors) {
                    // Keys often match C# property names; map them to your local field keys
                    const mapKey = (k: string) => {
                        const key = k.toLowerCase();
                        if (key.includes("minprice")) return "minPrice";
                        if (key.includes("maxprice")) return "maxPrice";
                        if (key.includes("stock")) return "stock";
                        if (key.includes("region")) return "region";
                        if (key.includes("harvestedat")) return "harvestedAt";
                        if (key.includes("potsize")) return "potSize";
                        if (key.includes("stemlength")) return "stemLength";
                        if (key.includes("productid")) return "productId";
                        if (key.includes("companyid")) return "companyId";
                        if (key.includes("userid")) return "userId";
                        return k; // fallback
                    };
                    for (const [k, arr] of Object.entries(normalized.errors)) {
                        const first = Array.isArray(arr) ? arr[0] : String(arr);
                        fieldErrorMap[mapKey(k)] = first;
                    }
                }

                // Compose a readable top message
                const top =
                    normalized.title ||
                    normalized.message ||
                    normalized.detail ||
                    `Server returned ${normalized.status}`;

                setErrors((prev) => ({ ...prev, ...fieldErrorMap, server: top }));
                setMessage("");
                return; // stop success flow
            }

            // Try to parse success JSON (if any). If body is empty, this won’t throw due to .text() path above.
            // You can keep it minimal if you don’t use the response content.
            // const success = await response.json().catch(() => null);

            setMessage("Productaanbod succesvol aangemaakt!");
            setFormData((prev) => ({
                productId: 0,
                minPrice: 0,
                maxPrice: undefined,
                stock: 0,
                region: "",
                harvestedAt: "",
                potSize: undefined,
                stemLength: undefined,
            }));
            setErrors({});
        } catch (err: unknown) {
            console.error(err);
            const message = err instanceof Error ? err.message : "Onbekende fout";
            setErrors((prev) => ({ ...prev, server: message }));
            setMessage("");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <ToevoegenLayout>
            <Form className={s.form} onSubmit={handleSubmit}>
                <h1 className={s.h1}>Productaanbod Aanmaken</h1>
                {errors.auth && <div className={s.error}>{errors.auth}</div>}
                {errors.companyId && <div className={s.error}>{errors.companyId}</div>}
                {errors.userId && <div className={s.error}>{errors.userId}</div>}

                <Form.Label>Product</Form.Label>
                <SearchableDropdown
                    label="Selecteer product"
                    items={products}
                    displayKey="name"
                    valueKey="id"
                    placeholder="Zoek product..."
                    onSelect={(p: ProductItem) => setFormData((prev) => ({ ...prev, productId: p.id }))}
                />
                {errors.productId && <div className={s.error}>{errors.productId}</div>}

                <Form.Label className="mt-3">Minimale prijs (per stuk)</Form.Label>
                <input
                    type="number"
                    className="form-control"
                    min={0}
                    step="0.01"
                    value={formData.minPrice}
                    onChange={(e) => setFormData((prev) => ({ ...prev, minPrice: Number(e.target.value) }))}
                />
                {errors.minPrice && <div className={s.error}>{errors.minPrice}</div>}

                <Form.Label className="mt-3">Maximale prijs (optioneel)</Form.Label>
                <input
                    type="number"
                    className="form-control"
                    min={0}
                    step="0.01"
                    value={formData.maxPrice ?? ""}
                    onChange={(e) => {
                        const v = e.target.value;
                        setFormData((prev) => ({ ...prev, maxPrice: v === "" ? undefined : Number(v) }));
                    }}
                />

                <Form.Label className="mt-3">Voorraad</Form.Label>
                <input
                    type="number"
                    className="form-control"
                    min={0}
                    value={formData.stock}
                    onChange={(e) => setFormData((prev) => ({ ...prev, stock: Number(e.target.value) }))}
                />
                {errors.stock && <div className={s.error}>{errors.stock}</div>}

                <Form.Label className="mt-3">Regio</Form.Label>
                <input
                    type="text"
                    className="form-control"
                    value={formData.region}
                    onChange={(e) => setFormData((prev) => ({ ...prev, region: e.target.value }))}
                />
                {errors.region && <div className={s.error}>{errors.region}</div>}

                <Form.Label className="mt-3">Oogst-datum en tijd</Form.Label>
                <DateSelect
                    placeholder="Selecteer datum en tijd"
                    useTime={true}
                    label=""
                    onSelect={(iso: string | null) => setFormData((prev) => ({ ...prev, harvestedAt: iso ?? "" }))}
                />
                {errors.harvestedAt && <div className={s.error}>{errors.harvestedAt}</div>}

                <Form.Label className="mt-3">Potmaat (optioneel)</Form.Label>
                <input
                    type="number"
                    className="form-control"
                    step="0.01"
                    value={formData.potSize ?? ""}
                    onChange={(e) => {
                        const v = e.target.value;
                        setFormData((prev) => ({ ...prev, potSize: v === "" ? undefined : Number(v) }));
                    }}
                />

                <Form.Label className="mt-3">Steellengte (optioneel)</Form.Label>
                <input
                    type="number"
                    className="form-control"
                    step="0.01"
                    value={formData.stemLength ?? ""}
                    onChange={(e) => {
                        const v = e.target.value;
                        setFormData((prev) => ({ ...prev, stemLength: v === "" ? undefined : Number(v) }));
                    }}
                />

                <Button
                    variant="primary"
                    type="submit"
                    label={isSubmitting ? "Opslaan..." : "Aanmaken"}
                    disabled={isSubmitting || isLoadingUser}
                />

                {message && <p style={{ marginTop: "1rem" }}>{message}</p>}
            </Form>
        </ToevoegenLayout>
    );
}