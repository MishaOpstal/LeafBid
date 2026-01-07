import ProductTable from "@/components/productTable/productTable";
import Header from "@/components/header/header";
import { Roles } from "@/enums/Roles";
import Chart from "@/components/chart/chart";
export default function Page() {
    return (
        <>
        <Header />
            <main style={{ height: 'calc(100dvh - 100px)' }} className="d-flex flex-row justify-content-center align-items-center p-4">
                <Chart />
                <ProductTable userRoles={Roles.Provider} />
            </main>
        </>
    );
}