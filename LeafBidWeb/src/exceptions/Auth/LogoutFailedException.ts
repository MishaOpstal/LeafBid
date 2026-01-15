import Exception from "@/exceptions/Exception";

interface LogoutFailedException extends Exception {
    name: "LogoutFailedException";
    message: "User logout failed.";
}

export default function LogoutFailedException(msg: string) {
    const error = new Error(msg) as LogoutFailedException;
    error.name = "LogoutFailedException";
    error.message = "User logout failed.";
    error.getMessage = () => error.message;
    return error;
}