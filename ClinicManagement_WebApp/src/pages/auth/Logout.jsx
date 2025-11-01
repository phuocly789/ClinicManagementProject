import { use, useEffect } from "react";
import authService from "../../services/authService";

const Logout = () => {
    useEffect(() => {
        authService.logout();
        window.location.href = "/login";
    }, []);
    return null;
};

export default Logout;

//