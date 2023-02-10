import axios from "axios";

export const axiosInstance = axios.create({
    baseURL: "http://localhost:5127",
    withCredentials: true
})

axiosInstance.interceptors.request.use((config) => {
    const csrfCookie = getCookie("CSRF-TOKEN")
    if (csrfCookie) {
        config.headers["X-CSRF-TOKEN"] = csrfCookie
    }
    return config
})

axiosInstance.interceptors.response.use(function (response) {
    // Any status code that lie within the range of 2xx cause this function to trigger
    // Do something with response data
    return response;
  }, function (error) {
    if (error?.response?.status === 401) {
        window.location.href = "https://localhost:7288/Identity/Account/Login"
    }
    return Promise.reject(error);
  });

function getCookie(name: string) {
    console.log(document.cookie)
    const value = `; ${document.cookie}`;
    const parts: string[] = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop()?.split(';').shift();
}