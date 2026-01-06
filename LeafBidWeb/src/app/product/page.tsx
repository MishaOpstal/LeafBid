import ProductTable from "@/components/productTable/productTable";
import Header from "@/components/header/header";
import { Roles } from "@/enums/Roles";
export default function Page() {
    return (
        <>
        <Header />
            <main style={{ height: 'calc(100dvh - 100px)' }} className="d-flex flex-column justify-content-center align-items-center p-4">
                <ProductTable userRoles={Roles.Provider} />
            </main>
        </>
    );
}