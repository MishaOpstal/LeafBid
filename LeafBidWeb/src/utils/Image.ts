export function resolveImageSrc(picture?: string): string | undefined {
    if (!picture) {
        return undefined;
    }

    if (
        picture.startsWith("http://") ||
        picture.startsWith("https://") ||
        picture.startsWith("//")
    ) {
        return picture;
    }

    return `http://localhost:5001/uploads/${picture}`;
}