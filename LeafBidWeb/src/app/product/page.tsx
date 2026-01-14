import ProductTable from "@/components/ProductTable/ProductTable";
import Header from "@/components/Header/Header";
import { Roles } from "@/enums/Roles";
import Chart from "@/components/Chart/Chart";
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