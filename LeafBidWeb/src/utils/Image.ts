import config from "@/Config";

export function resolveImageSrc(picture?: string): string | undefined {
    if (!picture) {
        return undefined;
    }

    if (/^(https?:)?\/\//.test(picture)) {
        return picture;
    }

    return `${config.baseUrl}/${picture}`;
}