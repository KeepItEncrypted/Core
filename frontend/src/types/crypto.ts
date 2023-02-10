export interface SignerAndEncryptor {
    encryptor: CryptoKeyPair,
    signer: CryptoKeyPair,
}

export interface KeyEncryptionOutput {
    encryptedKey: string,
    public_encryption_key: string,
    public_verifier_key: string
}