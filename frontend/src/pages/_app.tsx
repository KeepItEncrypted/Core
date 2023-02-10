import '@/styles/globals.css'
import type {AppProps} from 'next/app'
import AppLayout from "@/components/AppLayout";
import {useEffect, useRef} from "react";
import {AntiforgeryApi, Configuration} from "@/client_api";
import {axiosInstance} from "@/api/exios";

export default function App({Component, pageProps}: AppProps) {
    const isTokenLoaded = useRef(false)

    useEffect(() => {
        if (isTokenLoaded.current) {
            return;
        }
        isTokenLoaded.current = true;
        new AntiforgeryApi(new Configuration(), "", axiosInstance).apiAntiforgeryTokenGet()
    })

    return (
        <AppLayout>
            <Component {...pageProps} />
        </AppLayout>
    )
}
