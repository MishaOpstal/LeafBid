import s from '@/components/veilingInfo/velinginfo.module.css';
import {parseDate, RegisteredProduct} from "@/types/Product/RegisteredProducts";
import {Image} from "react-bootstrap";
import Button from "@/components/input/Button";
import {resolveImageSrc} from "@/utils/image";

export default function BigInfoVeld({registeredProduct}: { registeredProduct: RegisteredProduct }) {
    return (
        <div
            className={`d-flex flex-column  ${s.wrapper}`}>
            <div className="d-flex flex-row gap-4">
                <Image
                    src={resolveImageSrc(registeredProduct.product.picture)}
                    alt={registeredProduct.product.name}
                    className={`mb-3 ${s.plaatje}`}
                />
                <div className={`d-flex flex-row gap-1 ${s.infoBox}`}>
                    <p>{registeredProduct.product.description}</p></div>
            </div>
            <div className={`d-flex flex-column gap-3 p-3 ${s.tekstcontainer}`}>
                <h2>{registeredProduct.product.name}</h2>
                <p>Aantal: {registeredProduct.stock}</p>
                <p>Geoogst: {parseDate(registeredProduct.harvestedAt ?? "")}</p>
                <p>Leverancier: {registeredProduct.providerUserName}</p>
                <p>Regio Oorsprong: {registeredProduct.region}</p>
            </div>
            <Button label="Koop Product" variant="primary" type="button" className={s.knop}/>
        </div>
    );
}
