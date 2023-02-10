export function getMessageEncoding(text: string) {
    let message = text;
    let enc = new TextEncoder();
    return enc.encode(message);
}

export async function decryptAesKey(bufferEncryptedAes: Uint8Array, privateRsaKey: CryptoKey): Promise<ArrayBuffer | null> {
    try {
        return await window.crypto.subtle.decrypt(
            {name: "RSA-OAEP"},
            privateRsaKey,
            bufferEncryptedAes
        )
    } catch (e) {
        console.error(e)
        return null
    }
}

export async function decryptMessage(key: CryptoKey, ciphertext: Uint8Array, iv: ArrayBuffer): Promise<string | null> {
    try {
        const decryptedMessage = await window.crypto.subtle.decrypt({ name: "AES-GCM", iv }, key, Buffer.from(ciphertext))
        return new TextDecoder().decode(decryptedMessage);
    } catch (e) {
        console.error(e)
        return null
    }
}

export async function decryptFileFromBuffer(key: CryptoKey, ciphertext: ArrayBuffer, iv: ArrayBuffer): Promise<ArrayBuffer> {
    try {
        return await window.crypto.subtle.decrypt({ name: "AES-GCM", iv }, key, ciphertext);
    } catch (e) {
        console.error(e)
        return null
    }
}