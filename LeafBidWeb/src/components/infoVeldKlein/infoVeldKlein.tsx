import s from './infoVeldKlein.module.css';
import {parsePrice, RegisteredProduct} from "@/types/Product/RegisteredProducts";
import {Image} from "react-bootstrap";
import {resolveImageSrc} from "@/utils/image";

export default function InfoVeld({ registeredProduct }: { registeredProduct: RegisteredProduct }) {
    return (
        <div className={`d-flex align-items-center gap-3 ${s.textContainer}`}>
            <Image
                src={resolveImageSrc(registeredProduct.product.picture)}
                alt={registeredProduct.product.name}
                className={`img-fluid ${s.image}`}
            />
            <div className={`d-flex flex-column justify-content-center ps-3 gap-2 ${s.omschrijving}`}>
                <h4 className="fw-bold">{registeredProduct.product.name}</h4>
                <p>{parsePrice(registeredProduct.minPrice ?? 0)}</p>
            </div>
        </div>
    );
}
