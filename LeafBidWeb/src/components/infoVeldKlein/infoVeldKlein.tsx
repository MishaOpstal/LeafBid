import s from './infoVeldKlein.module.css';
import {parsePrice, Product} from "@/types/Product/Product";
import {Image} from "react-bootstrap";

export default function InfoVeld({ product }: { product: Product }) {
    const imageSrc = `http://localhost:5001/uploads/${product.picture}`;

    return (
        <div className={`d-flex align-items-center gap-3 ${s.textContainer}`}>
            <Image
                src={imageSrc}
                alt={product.name}
                className={`img-fluid ${s.image}`}
            />
            <div className={`d-flex flex-column justify-content-center ps-3 gap-2 ${s.omschrijving}`}>
                <h4 className="fw-bold">{product.name}</h4>
                <p>{parsePrice(product.minPrice ?? 0)}</p>
            </div>
        </div>
    );
}
