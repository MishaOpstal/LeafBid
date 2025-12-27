import Exception from "@/exceptions/Exception";

interface RoleFetchFailedException extends Exception {
    name: "RoleFetchFailedException";
    message: "Fetching roles failed.";
}

export default function RoleFetchFailedException(msg: string) {
    const error = new Error(msg) as RoleFetchFailedException;
    error.name = "RoleFetchFailedException";
    error.message = "Fetching roles failed.";
    error.getMessage = () => error.message;
    return error;
}

export function isRoleFetchFailedException(e: unknown): e is RoleFetchFailedException {
    return e instanceof Error && (e as RoleFetchFailedException).name === "RoleFetchFailedException" && typeof (e as RoleFetchFailedException).getMessage === "function";
}