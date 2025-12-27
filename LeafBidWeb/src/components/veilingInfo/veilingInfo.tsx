import s from '@/components/veilingInfo/velinginfo.module.css';
import {parseDate, Product} from "@/types/Product/Product";
import {Image} from "react-bootstrap";
import Button from "@/components/input/Button";

export default function BigInfoVeld({product}: { product: Product }) {
    const imageSrc = `http://localhost:5001/uploads/${product.picture}`;

    return (
        <div
            className={`d-flex flex-column  ${s.wrapper}`}>
            <div className="d-flex flex-row gap-4">
                <Image
                    src={imageSrc}
                    alt={product.name}
                    className={`mb-3 ${s.plaatje}`}
                />
                <div className={`d-flex flex-row gap-1 ${s.infoBox}`}>
                    <p>{product.description}</p></div>
            </div>
            <div className={`d-flex flex-column gap-3 p-3 ${s.tekstcontainer}`}>
                <h2>{product.name}</h2>
                <p>Aantal: {product.stock}</p>
                <p>Geoogst: {parseDate(product.harvestedAt ?? "")}</p>
                <p>Leverancier: {product.providerUserName}</p>
                <p>Regio Oorsprong: {product.region}</p>
            </div>
            <Button label="Koop Product" variant="primary" type="button" className={s.knop}/>
        </div>
    );
}
