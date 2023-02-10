import {Title} from "@/components/common/Title";
import {useAtom} from "jotai";
import {idOfUploadedKeyAtom, signerAtom} from "@/state/crypto";
import {encryptAesKey, encryptMessage} from "@/cryptoUtils/encrypt";
import {FormEvent, useState} from "react";
import {TextEncoder} from "next/dist/compiled/@edge-runtime/primitives/encoding";
import FormData from "form-data";
import {
    Configuration,
    CreateBundle,
    CreateRecipient,
    EncryptedFileDto,
    EncryptedKeyDto,
    PublicKeyPairApi,
    TransfersApi
} from "@/client_api";
import {buff_to_base64} from "@/cryptoUtils/keyGeneration";
import {axiosInstance} from "@/api/exios";
import DragDropFile from "@/components/common/DragAndDrop";


export function Dashboard() {
    const [signer] = useAtom(signerAtom)

    const [uploadedFiles, setUploadedFiles] = useState<File[]>([])
    const [userId, setUserId] = useState("")
    const [currentKeyId] = useAtom(idOfUploadedKeyAtom)

    const addFile = async (event: FormEvent<HTMLInputElement>) => {
        setUploadedFiles(Array.from(event.target.files))
    }


    const encryptAndSend = async (e: { preventDefault: () => void; }) => {
        e.preventDefault()
        const publicKeysOfReceiver = await new PublicKeyPairApi(new Configuration(), "", axiosInstance).apiPublicKeyPairForUserUserIdGet(userId)
        let generalAesKey = await window.crypto.subtle.generateKey(
            {
                name: "AES-GCM",
                length: 256
            },
            true,
            ["encrypt", "decrypt"]
        );
        const iv = window.crypto.getRandomValues(new Uint8Array(16));

        const exportedAesKeyBuffer = await window.crypto.subtle.exportKey("raw", generalAesKey)
        const encryptedFiles: EncryptedFileDto[] = []
        const encryptedKeys: EncryptedKeyDto[] = []
        for (const file of uploadedFiles) {
            const fileBuffer = await file.arrayBuffer()
            const encryptedFile = await encryptMessage(fileBuffer, generalAesKey, iv)
            const signature = await window.crypto.subtle.sign({
                    name: "ECDSA",
                    hash: {name: "SHA-384"},
                },
                signer?.privateKey!,
                encryptedFile!
            )
            if (encryptedFile === null) {
                return
            }
            const textEncoder = new TextEncoder()
            const encryptedFileName = await encryptMessage(textEncoder.encode(file.name), generalAesKey, iv)
            if (encryptedFileName === null) {
                return
            }

            const base64EncryptedFilename = buff_to_base64(new Uint8Array(encryptedFileName))
            encryptedFiles.push({
                encryptedFileName: base64EncryptedFilename,
                formFile: new File([encryptedFile], "encrypted_file"),
                signature: buff_to_base64(Buffer.from(signature)),
            })
        }
        for (const key of publicKeysOfReceiver.data) {
            const importedPublicKey = await window.crypto.subtle.importKey("jwk", JSON.parse(key.publicEncryptionKey!), {
                    name: "RSA-OAEP",
                    hash: "SHA-256"
                },
                true,
                ["encrypt"])

            const encryptedGeneralAesKey = await encryptAesKey(importedPublicKey, exportedAesKeyBuffer)
            const base64Iv = buff_to_base64(iv)
            const base64EncryptedGeneralAesKey = buff_to_base64(new Uint8Array(encryptedGeneralAesKey!))
            const base64IvAndAes = base64Iv + base64EncryptedGeneralAesKey

            encryptedKeys.push({
                publicKeyPairId: key.id!,
                encryptedAesKey: base64IvAndAes
            })
        }

        const recipient: CreateRecipient = {
            userId: userId,
            encryptedKeys: encryptedKeys
        }
        const payload: CreateBundle = {
            encryptedMessage: "haha",
            usedPublicKeyId: currentKeyId!,
            recipients: [recipient]
        }
        let response
        try {
            response = await new TransfersApi(new Configuration(), '', axiosInstance).apiTransfersCreateBundlePost(payload)
        } catch (e) {
            console.error(e)
            return
        }
        const createdBundleId = response.data.createdBundleId

        const fileUploads = []
        for (const encryptedFile of encryptedFiles) {
            const formData = new FormData()
            formData.append("FormFile", encryptedFile.formFile)
            formData.append("EncryptedFileName", encryptedFile.encryptedFileName)
            formData.append("Signature", encryptedFile.signature)

            const promise = axiosInstance.post(`/api/Transfers/Bundle/${createdBundleId}/AddFile`, formData, {
                headers: {
                    "Content-Type": "multipart/form-data",
                },
            })
            fileUploads.push(promise)
        }
        await Promise.all(fileUploads)
        setUploadedFiles([])
    }
    return (
        <div>
            <Title text={"Upload items"} isSubtitle={false}/>

            <form className={"flex flex-col"} onSubmit={encryptAndSend}>
                <input type={"text"} required placeholder={"user id"} value={userId}
                       onChange={(event) => setUserId(event.target.value)}/>
                <DragDropFile multipleFiles={true}
                              filesLoaded={(files) => setUploadedFiles([...uploadedFiles, ...files])}/>
                <button type={"submit"}>Encrypt and send</button>
            </form>
            {uploadedFiles.map(file => {
                return (
                    <div key={file.name} className={"py-1"}>{file.name} - size: {formatFileSize(file.size)}</div>
                )
            })}
            {uploadedFiles.length > 0 && (<div>
                <button onClick={() => setUploadedFiles([])}>Clear files</button>
            </div>)}
        </div>
    )
}

const formatFileSize = (bytes: number): string => {
    if (bytes > 1_000_000_000) {
        return (bytes / 1_000_000_000).toFixed(2) + " GB"
    } else if (bytes > 1_000_000) {
        return (bytes / 1_000_000).toFixed(2) + " MB"
    } else if (bytes > 1_000) {
        return (bytes / 1_000).toFixed(2) + " KB"
    } else {
        return bytes + " bytes"
    }
}