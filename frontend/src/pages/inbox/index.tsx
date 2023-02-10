import {useEffect, useRef, useState} from "react";
import styles from "@/styles/Home.module.css";
import {Configuration, InboxApi, InboxItem} from "@/client_api";
import {axiosInstance} from "@/api/exios";
import {useRouter} from "next/router";
import Container from "@/components/common/Container";
import {Title} from "@/components/common/Title";

export default function Inbox() {

    const hasInboxLoaded = useRef(false)
    const [inboxItems, setInboxItems] = useState<InboxItem[]>([])
    const router = useRouter()

    useEffect(() => {
        if (hasInboxLoaded.current) {
            return
        }
        hasInboxLoaded.current = true
        const loadInbox = async () => {
            const response = await new InboxApi(new Configuration(), '', axiosInstance).apiInboxGet()
            setInboxItems(response.data)
        }
        loadInbox()


    }, [hasInboxLoaded])



    return (
        <>
            <main>
                <Container>
                    <div className="flex flex-col container">
                        <Title isSubtitle={false} text={"Inbox"} />
                        <div>
                            {inboxItems.map(item => {
                                return (
                                    <div onClick={async () => router.push(`/inbox/${item.id}`)}
                                         className={"py-5 px-5 mb-2 bg-gray-100 cursor-pointer"} key={item.id}>{item.message} -
                                        Id: {item.id} - BundleId: {item.bundleId}</div>
                                )
                            })}
                        </div>
                    </div>
                </Container>
            </main>
        </>
    )
}