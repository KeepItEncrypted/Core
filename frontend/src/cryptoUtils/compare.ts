export async function compareKeys(key1: CryptoKey, key2: CryptoKey): Promise<boolean> {
    const keyData1 = new DataView(await crypto.subtle.exportKey('spki', key1));
    const keyData2 = new DataView(await crypto.subtle.exportKey('spki', key2));
    for (let i = 0; i < keyData1.byteLength; i++) {
        if (keyData1.getUint8(i) !== keyData2.getUint8(i)) return false;
    }
    return true

}