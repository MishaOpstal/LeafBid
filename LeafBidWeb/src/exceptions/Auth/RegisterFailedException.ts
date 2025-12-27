import Exception from "@/exceptions/Exception";

interface RegisterFailedException extends Exception {
    name: "RegisterFailedException";
    message: "User registration failed.";
}

export default function RegisterFailedException(msg: string) {
    const error = new Error(msg) as RegisterFailedException;
    error.name = "RegisterFailedException";
    error.message = "User registration failed.";
    error.getMessage = () => error.message;
    return error;
}

export function isRegisterFailedException(e: unknown): e is RegisterFailedException {
    return e instanceof Error && (e as RegisterFailedException).name === "RegisterFailedException" && typeof (e as RegisterFailedException).getMessage === "function";
}