import axios from "axios";
import _ from "lodash";
import NProgress from "nprogress";
import "nprogress/nprogress.css";

const API_BASE_URL = 'https://clinicLaravel.lmp.id.vn';
NProgress.configure({ showSpinner: false });
const instanceReceptionist = axios.create({
    baseURL: API_BASE_URL,
    withCredentials: false,
});

instanceReceptionist.interceptors.request.use((config) => {
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


instanceReceptionist.interceptors.response.use(
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

export default instanceReceptionist;