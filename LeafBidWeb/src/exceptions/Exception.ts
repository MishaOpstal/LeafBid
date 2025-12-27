interface Exception {
    name: string;
    message: string;
    stack?: string;
    getMessage: () => string;
}

export default Exception;