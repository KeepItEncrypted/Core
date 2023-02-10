import {atom} from "jotai";

export const signerAtom = atom<CryptoKeyPair | null>(null)
export const encryptorAtom = atom<CryptoKeyPair | null>(null)

export const signerPublicKeysAtom = atom<Record<number, CryptoKey> | null>(null)
export const idOfUploadedKeyAtom = atom<string | null>(null)

