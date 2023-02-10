import {useRouter} from "next/router";
import {Title} from "@/components/common/Title";
import Container from "@/components/common/Container";
import {useEffect, useRef, useState} from "react";
import {Configuration, InboxApi, InboxItemDetail, PublicKeyPairApi} from "@/client_api";
import {axiosInstance} from "@/api/exios";
import {compareKeys} from "@/cryptoUtils/compare";
import {base64_to_buf} from "@/cryptoUtils/keyGeneration";
import {decryptAesKey, decryptFileFromBuffer, decryptMessage} from "@/cryptoUtils/decrypt";
import {useAtom} from "jotai";
import {encryptorAtom} from "@/state/crypto";

export default function ReceivedItem() {
    const router = useRouter()
    const {id} = router.query;
    const hasItemLoaded = useRef(false)
    const [encryptor] = useAtom(encryptorAtom)
    const [inboxItemDetails, setInboxItemDetails] = useState<InboxItemDetail | null>(null)
    const [publicKeyForVerifying, setPublicKeyForVerifying] = useState<CryptoKey | null>(null)
    const [decryptedFilenameMap, setDecryptedFilenameMap] = useState<Record<string, string>>({})
    const [aesKey, setAesKey] = useState<CryptoKey>()
    const [aesIv, setAesIv] = useState<Uint8Array>()
    const [isErrorWithSignatureVerification, setIsErrorWithSignatureVerification] = useState(false)


    const downloadAndDecryptFile = async (storageUrl: string, fileName: string, signature: string) => {
        setIsErrorWithSignatureVerification(false)
        if (!aesKey || !aesIv) {
            return
        }
        const data = await fetch(`https://localhost:7288${storageUrl}`)
        const blob = await data.blob()
        const fileBuffer = await blob.arrayBuffer()
        const signatureBuffer = base64_to_buf(signature).buffer
        const isValidSignature = await window.crypto.subtle.verify({
            name: "ECDSA",
            hash: {name: "SHA-384"},
        }, publicKeyForVerifying!, signatureBuffer, fileBuffer)
        console.log()
        if (!isValidSignature) {
            setIsErrorWithSignatureVerification(true)
            return
        }

        const decryptedFile = await decryptFileFromBuffer(aesKey, fileBuffer, aesIv)
        const dataView = new DataView(decryptedFile);
        const fileBlob = new Blob([dataView]);
        const downloadUrl = URL.createObjectURL(fileBlob);
        const a = document.createElement('a');
        a.href = downloadUrl;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        URL.revokeObjectURL(downloadUrl);
        a.remove()
    }


    useEffect(() => {
        const loadInboxItem = async (recipientId: number) => {
            if (encryptor === null) {
                return
            }
            const response = await new InboxApi(new Configuration(), '', axiosInstance).apiInboxRecipientIdGet(recipientId)
            await setInboxItemDetails(response.data)
            const filenameMap: Record<string, string> = {}
            setDecryptedFilenameMap(filenameMap)

            for (const dk of response.data?.decryptorKeys ?? []) {
                const importedPublicKey = await window.crypto.subtle.importKey("jwk", JSON.parse(dk.publicKeyUsedForEncryption ?? ""), {
                        name: "RSA-OAEP",
                        hash: "SHA-256"
                    },
                    true,
                    ["encrypt"])
                const isTheSameAsUploadedKey = await compareKeys(importedPublicKey, encryptor?.publicKey)
                if (isTheSameAsUploadedKey) {
                    const base64Iv = dk.encryptedSymmetricKey?.slice(0, 24)
                    const base64Aes = dk.encryptedSymmetricKey?.slice(24)
                    const bufferAes = base64_to_buf(base64Aes ?? "")
                    const bufferIv = base64_to_buf(base64Iv ?? "")
                    const aesKey = await decryptAesKey(bufferAes, encryptor?.privateKey)
                    const importedAesKey = await window.crypto.subtle.importKey("raw", aesKey!, {
                            name: "AES-GCM",
                            length: 256
                        },
                        true,
                        ["encrypt", "decrypt"])
                    setAesKey(importedAesKey)
                    setAesIv(bufferIv)
                    for (const attachment of response.data?.attachments ?? []) {
                        const decryptedFileName = await decryptMessage(importedAesKey, base64_to_buf(attachment.fileName ?? ""), bufferIv)
                        filenameMap[attachment.fileName ?? ""] = decryptedFileName ?? ""
                    }
                    setDecryptedFilenameMap({...filenameMap})
                }
            }
        }
        if (hasItemLoaded.current || encryptor === null) {
            return
        }
        hasItemLoaded.current = true
        loadInboxItem(id)
    }, [hasItemLoaded, encryptor, id])

    useEffect(() => {
        if (inboxItemDetails === null) {
            return
        }
        const loadKey = async () => {
            const response = await new PublicKeyPairApi(new Configuration(), '', axiosInstance)
                .apiPublicKeyPairPublicKeyIdGet(inboxItemDetails.publicKeyIdForSignature!)
            const key: JsonWebKey = JSON.parse(response.data.publicVerifierKey!)
            const importedKey = await window.crypto.subtle.importKey("jwk", key, {
                name: "ECDSA",
                namedCurve: "P-384"
            }, true, ["verify"])
            setPublicKeyForVerifying(importedKey)
        }
        loadKey()

    }, [inboxItemDetails])

    return (
        <Container>
            {inboxItemDetails !== null && (
                <div>
                    <Title isSubtitle={false} text={"Message"}/>
                    <p>{inboxItemDetails.message}</p>
                    <div>
                        {isErrorWithSignatureVerification && (
                            <div className={"mt-5"}>
                                <Title isSubtitle={true} text={"Errors"}></Title>
                                <p className={"py-5 px-5 bg-red-300"}>This document has been tampered with. Canceling download.</p>
                            </div>
                        )}
                        <h2 className="text-3xl font-extrabold leading-9 tracking-tight text-gray-900 dark:text-gray-100 sm:text-2xl sm:leading-10 md:text-4xl md:leading-1 mt-10">Attachments</h2>
                        <div className={"flex flex-col"}>
                            {inboxItemDetails?.attachments?.map(a => {
                                return (
                                    <div className={"py-2 px-5 mb-1 bg-gray-100 flex justify-between"}
                                         key={a.fileName}>
                                        <p>{decryptedFilenameMap[a.fileName ?? ""]}</p>
                                        <button
                                            onClick={() => downloadAndDecryptFile(a.storageLocation ?? "", decryptedFilenameMap[a.fileName ?? ""], a.signature!)}>Download
                                        </button>
                                    </div>
                                )
                            })}
                        </div>
                    </div>
                </div>
            )}
            {inboxItemDetails === null && (
                <div>
                    <Title isSubtitle={true} text={"Upload your cryptographic key to load received files"}/>
                </div>
            )}
        </Container>
    )
}