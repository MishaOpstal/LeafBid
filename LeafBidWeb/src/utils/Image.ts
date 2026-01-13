export function resolveImageSrc(picture?: string): string | undefined {
    if (!picture) {
        return undefined;
    }

    if (/^(https?:)?\/\//.test(picture)) {
        return picture;
    }

    return `http://localhost:5001/uploads/${picture}`;
}