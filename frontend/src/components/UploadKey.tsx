import {FormEvent, useState} from "react";
import {SignerAndEncryptor} from "@/types/crypto";
import {base64_to_buf, createRSAKey, deriveKey, encryptNewKey, getPasswordKey} from "@/cryptoUtils/keyGeneration";
import {axiosInstance} from "@/api/exios";
import {useAtom} from "jotai";
import {encryptorAtom, idOfUploadedKeyAtom, signerAtom, signerPublicKeysAtom} from "@/state/crypto";
import {compareKeys} from "@/cryptoUtils/compare";
import {Title} from "@/components/common/Title";

interface UploadKeyProps {
    onKeyUpload: (pair: SignerAndEncryptor) => void
}

enum KeyCreationFlow {
    Upload,
    GenerateNew,
    DecryptKey,
    Ready
}

export function UploadKey({onKeyUpload}: UploadKeyProps) {
    const [flow, setFlow] = useState(KeyCreationFlow.Upload)
    const [password, setPassword] = useState("")
    const [encryptedKey, setEncryptedKey] = useState("")
    const [signerAndEncryptor, setSignerAndEncryptor] = useState<SignerAndEncryptor>()
    const [isErrorWithDecryption, setIsErrorWithDecryption] = useState(false)
    const [, setSigner] = useAtom(signerAtom)
    const [, setEncryptor] = useAtom(encryptorAtom)
    const [signerPublicKeys] = useAtom(signerPublicKeysAtom)
    const [, setCurrentKeyId] = useAtom(idOfUploadedKeyAtom)

    const receiveKey = (event: FormEvent<HTMLInputElement>) => {
        // @ts-ignore
        const file = event.target.files[0];
        const reader = new FileReader();
        reader.readAsText(file);
        reader.onload = () => {
            setEncryptedKey(reader.result as string)
            setFlow(KeyCreationFlow.DecryptKey)
        }
    }

    const generateNewKey = async () => {
        const signerAndEncryptor = await createRSAKey()
        setSignerAndEncryptor(signerAndEncryptor)
        setFlow(KeyCreationFlow.GenerateNew)
    }

    const downloadKey = (key: File) => {
        const url = window.URL.createObjectURL(key);
        const a = document.createElement("a");
        a.href = url;
        a.download = "key.txt";
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }


    const finishKeyGeneration = async (event) => {
        event.preventDefault()
        if (!signerAndEncryptor) {
            return
        }
        const result = await encryptNewKey(signerAndEncryptor, password)
        if (result === null) {
            return
        }
        await axiosInstance.post("/api/PublicKeyPair", {
            publicVerifierKey: result.public_verifier_key,
            publicEncryptionKey: result.public_encryption_key
        })
        const file = new File([result?.encryptedKey], "key.txt")
        downloadKey(file)
        setFlow(KeyCreationFlow.Upload)
        setPassword("")
    }

    const setIdOfUploadedKey = async (publicKey: CryptoKey): Promise<void> => {
        for (const index of Object.keys(signerPublicKeys ?? [])) {
            if (await compareKeys(publicKey, signerPublicKeys![index])) {
                setCurrentKeyId(index);
                break
            }
        }
    }

    const decryptKey = async (encryptedData: string, password: string) => {
        setIsErrorWithDecryption(false)
        try {

            const encryptedDataBuff = base64_to_buf(encryptedData);
            const salt = encryptedDataBuff.slice(0, 16);
            const iv = encryptedDataBuff.slice(16, 16 + 32);
            const data = encryptedDataBuff.slice(16 + 32);
            const passwordKey = await getPasswordKey(password);
            const aesKey = await deriveKey(passwordKey, salt, ["decrypt"]);
            const decryptedContent = await window.crypto.subtle.decrypt(
                {
                    name: "AES-GCM",
                    iv: iv,
                },
                aesKey,
                data
            );
            const text = new TextDecoder().decode(decryptedContent);
            const keyInJsonForm = JSON.parse(text)
            const keyInCorrectJsonForm: SignerAndEncryptor = {
                signer: {
                    privateKey: await window.crypto.subtle.importKey("jwk", keyInJsonForm.signer.private, {
                        name: "ECDSA",
                        namedCurve: "P-384"
                    }, true, ["sign"]),
                    publicKey: await window.crypto.subtle.importKey("jwk", keyInJsonForm.signer.public, {
                        name: "ECDSA",
                        namedCurve: "P-384"
                    }, true, ["verify"]),
                },
                encryptor: {
                    privateKey: await window.crypto.subtle.importKey("jwk", keyInJsonForm.encryptor.private, {
                        name: "RSA-OAEP",
                        hash: "SHA-256"
                    }, true, ["decrypt"]),
                    publicKey: await window.crypto.subtle.importKey("jwk", keyInJsonForm.encryptor.public, {
                        name: "RSA-OAEP",
                        hash: "SHA-256"
                    }, true, ["encrypt"]),
                }
            }
            setSigner(keyInCorrectJsonForm.signer)
            setEncryptor(keyInCorrectJsonForm.encryptor)
            onKeyUpload(keyInCorrectJsonForm)
            setIdOfUploadedKey(keyInCorrectJsonForm.signer.publicKey)
            setFlow(KeyCreationFlow.Ready)
        } catch (e) {
            console.log(e)
            setIsErrorWithDecryption(true)
        } finally {
            setPassword("")
        }
    }

    if (flow === KeyCreationFlow.Upload) {
        return (
            <div>
                <Title isSubtitle={true} text={"Cryptographic key not uploaded"}></Title>
                <div className={"flex w-full md:w-1/2 items-center"}>
                    <div className={""}>
                        <label
                            htmlFor="file-upload"
                            className="relative cursor-pointer rounded-md bg-white font-medium text-indigo-600 focus-within:outline-none focus-within:ring-2 focus-within:ring-indigo-500 focus-within:ring-offset-2 hover:text-indigo-500"
                        >
                            <span>Upload key</span>
                            <input id="file-upload" name="file-upload" type="file" className="sr-only" onChange={receiveKey}/>
                        </label>
                    </div>
                    <div className={"flex ml-10 items-center"}>
                        <button className="bg-indigo-700 hover:bg-indigo-600 text-white p-2 mb-5 rounded-xl"
                                onClick={generateNewKey}>Generate a new one
                        </button>
                    </div>
                </div>
            </div>
        )
    } else if (flow === KeyCreationFlow.GenerateNew) {
        return (
            <div>
                <Title isSubtitle={true} text={"We need to encrypt your cryptographic key with a password"}></Title>
                <form className={"flex flex-col w-full md:w-1/2"} onSubmit={finishKeyGeneration}>
                    <input type="password" value={password} onChange={event => setPassword(event.target.value)}
                           autoFocus={true} required={true} className={"w-full md:w-1/2"} placeholder={"Password"}/>
                    <button type={"submit"}
                            className={"w-full md:w-1/2 bg-indigo-700 hover:bg-indigo-600 text-white p-2 mt-2 rounded-xl"}>Finish
                    </button>
                </form>
            </div>
        )
    } else if (flow === KeyCreationFlow.DecryptKey) {
        return (
            <div className={"w-1/3"}>
                <h1>Decrypt Key</h1>
                <form action="#" method={"POST"} className={"flex flex-col"} onSubmit={(event) => {
                    event.preventDefault()
                    decryptKey(encryptedKey, password)
                }}>
                    <input type="password" value={password} onChange={event => setPassword(event.target.value)}
                           autoFocus={true} required={true}/>
                    <button className={"bg-amber-400 mt-4 py-5"} disabled={password.length === 0}
                            type={"submit"}>Decrypt
                    </button>
                    {isErrorWithDecryption && (
                        <div>
                            <p>Error with decryption.</p>
                            <p>Is your password correct?</p>
                            <p>Is file correct?</p>
                        </div>
                    )}
                </form>
            </div>
        )
    } else if (flow === KeyCreationFlow.Ready) {
        return (
            <div className={"w-1/2  bg-green-300 py-5 px-4"}>
                <p>Your cryptographic keys are ready</p>
            </div>
        )
    } else {
        return null
    }

}