export function getMessageEncoding(text: string) {
    let message = text;
    let enc = new TextEncoder();
    return enc.encode(message);
}

export async function encryptMessage(file: ArrayBuffer, aesKey: CryptoKey, iv: Uint8Array): Promise<ArrayBuffer | null> {
    try {
        const cypherText = await window.crypto.subtle.encrypt(
            {name: "AES-GCM", iv: iv, length: 256},
            aesKey,
            file
        );
        return cypherText
    } catch (e) {
        console.error(e)
        return null
    }
}

export const encryptAesKey = async (publicKey: CryptoKey, aesKeyBuffer: ArrayBuffer) => {
    try {
        const cypherText = await window.crypto.subtle.encrypt(
            {name: "RSA-OAEP"},
            publicKey,
            aesKeyBuffer
        );
        return cypherText
    } catch (e) {
        console.error(e)
        return null
    }
}