import Exception from "@/exceptions/Exception";

interface LoginFailedException extends Exception {
    name: "LoginFailedException";
    message: "User login failed.";
}

export default function LoginFailedException(msg: string) {
    const error = new Error(msg) as LoginFailedException;
    error.name = "LoginFailedException";
    error.message = "User login failed.";
    error.getMessage = () => error.message;
    return error;
}

export function isLoginFailedException(e: unknown): e is LoginFailedException {
    return e instanceof Error && (e as LoginFailedException).name === "LoginFailedException" && typeof (e as LoginFailedException).getMessage === "function";
}