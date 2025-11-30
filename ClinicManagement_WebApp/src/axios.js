import axios from "axios";
import _ from "lodash";
import NProgress from "nprogress";
import "nprogress/nprogress.css";

const API_BASE_URL = 'http://clinicapi.lmp.id.vn/api';
NProgress.configure({ showSpinner: false });
const instance = axios.create({
    baseURL: API_BASE_URL,
    withCredentials: false,
});

instance.interceptors.request.use((config) => {
    NProgress.start();

    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, (error) => {
    NProgress.done();
    return Promise.reject(error);
});


instance.interceptors.response.use(
    (response) => {
        NProgress.done();
        return response.data;
    },
    (error) => {
        NProgress.done();
        if (error.response?.status === 401) {
            localStorage.removeItem("token");
            localStorage.removeItem("roles");
            window.location.href = "/login";
        }
        return Promise.reject(error);
    }
);

export default instance;