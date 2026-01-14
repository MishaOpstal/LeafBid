import Exception from "@/exceptions/Exception";

interface CompanyFetchFailedException extends Exception {
    name: "CompanyFetchFailedException";
    message: "Fetching companies failed.";
}

export default function CompanyFetchFailedException(msg: string) {
    const error = new Error(msg) as CompanyFetchFailedException;
    error.name = "CompanyFetchFailedException";
    error.message = "Fetching companies failed.";
    error.getMessage = () => error.message;
    return error;
}

export function isCompanyFetchFailedException(e: unknown): e is CompanyFetchFailedException {
    return e instanceof Error && (e as CompanyFetchFailedException).name === "CompanyFetchFailedException" && typeof (e as CompanyFetchFailedException).getMessage === "function";
}