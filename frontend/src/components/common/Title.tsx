export interface TitleProps {
    isSubtitle: boolean
    text: string
}
export function Title({isSubtitle = false, text}: TitleProps) {
    if (isSubtitle) {
        return (
            <h2 className={"text-2xl font-bold my-2"}>{text}</h2>
        )
    } else {
        return (
            <h1 className={"text-3xl font-bold my-5 md:text-4xl lg:text-5xl"}>{text}</h1>
        )
    }
}