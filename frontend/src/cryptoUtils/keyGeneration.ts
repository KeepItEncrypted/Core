import {CryptoKey} from "next/dist/compiled/@edge-runtime/primitives/crypto";
import {KeyEncryptionOutput, SignerAndEncryptor} from "@/types/crypto";

export const createRSAKey = async (): Promise<SignerAndEncryptor> => {
    const encryptor = await window.crypto.subtle.generateKey(
        {
            name: "RSA-OAEP",
            modulusLength: 4096,
            publicExponent: new Uint8Array([1, 0, 1]),
            hash: "SHA-256"
        },
        true,
        ["encrypt", "decrypt"]
    );
    const signer = await window.crypto.subtle.generateKey(
        {
            name: "ECDSA",
            namedCurve: "P-384"
        },
        true,
        ["sign", "verify"]
    );
    return {encryptor, signer};
}

export const getPasswordKey = async (password: string): Promise<CryptoKey> => {
    let enc = new TextEncoder();
    return await window.crypto.subtle.importKey(
        "raw",
        enc.encode(password),
        "PBKDF2",
        false,
        ["deriveKey"]
    )
}

export const deriveKey = (passwordKey: CryptoKey, salt: Uint8Array, keyUsage: KeyUsage[]) =>
    window.crypto.subtle.deriveKey(
        {
            name: "PBKDF2",
            salt: salt,
            iterations: 250000,
            hash: "SHA-256",
        },
        passwordKey,
        {name: "AES-GCM", length: 256},
        false,
        keyUsage
    );

export async function encryptNewKey(secretData: SignerAndEncryptor, password: string): Promise<KeyEncryptionOutput | null> {
    const keyInJsonForm = {
        encryptor: {
            public: await window.crypto.subtle.exportKey("jwk", secretData.encryptor.publicKey),
            private: await window.crypto.subtle.exportKey("jwk", secretData.encryptor.privateKey)
        },
        signer: {
            public: await window.crypto.subtle.exportKey("jwk", secretData.signer.publicKey),
            private: await window.crypto.subtle.exportKey("jwk", secretData.signer.privateKey)
        }
    }
    const keyInInStringForm = JSON.stringify(keyInJsonForm)
    try {
        const salt = window.crypto.getRandomValues(new Uint8Array(16));
        const iv = window.crypto.getRandomValues(new Uint8Array(32));
        const passwordKey = await getPasswordKey(password);
        const aesKey = await deriveKey(passwordKey, salt, ["encrypt"]);
        const encryptedContent = await window.crypto.subtle.encrypt(
            {
                name: "AES-GCM",
                iv: iv,
                length: 256
            },
            aesKey,
            new TextEncoder().encode(keyInInStringForm)
        );
        const encryptedContentArr = new Uint8Array(encryptedContent);
        let buff = new Uint8Array(
            salt.byteLength + iv.byteLength + encryptedContentArr.byteLength
        );
        buff.set(salt, 0);
        buff.set(iv, salt.byteLength);
        buff.set(encryptedContentArr, salt.byteLength + iv.byteLength);
        return {
            encryptedKey: buff_to_base64(buff),
            public_encryption_key: JSON.stringify(keyInJsonForm.encryptor.public),
            public_verifier_key: JSON.stringify(keyInJsonForm.signer.public)
        }
    } catch (e) {
        console.error(e)
    }
    return null
}

// @ts-ignore
export const buff_to_base64 = (buff: Uint8Array) => btoa(String.fromCharCode.apply(null, buff));
// @ts-ignore
export const base64_to_buf = (b64: string) => Uint8Array.from(atob(b64), (c) => c.charCodeAt(null))