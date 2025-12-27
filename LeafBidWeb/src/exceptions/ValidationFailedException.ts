import Exception from "@/exceptions/Exception";

interface ValidationFailedException extends Exception {
    name: "ValidationFailedException";
    message: "Validation failed.";
}

export default function ValidationFailedException(msg: string) {
    const error = new Error(msg) as ValidationFailedException;
    error.name = "ValidationFailedException";
    error.message = "Validation failed.";
    error.getMessage = () => error.message;
    return error;
}

export function isValidationFailedException(e: unknown): e is ValidationFailedException {
    return e instanceof Error && (e as ValidationFailedException).name === "ValidationFailedException" && typeof (e as ValidationFailedException).getMessage === "function";
}