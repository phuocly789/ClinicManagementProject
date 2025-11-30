import React, { useEffect } from "react";
import { useNavigate, useLocation, Outlet } from "react-router-dom";
import authService from "../../services/authService";
import { path } from "../../utils/constant";

const RedirectIfLoggedIn = () => {
    const navigate = useNavigate();
    const location = useLocation(); // THÊM ĐỂ LẤY ĐƯỜNG DẪN HIỆN TẠI
    const token = authService.getToken();
    const roles = authService.getRoles();

    useEffect(() => {
        if (!token || roles.length === 0) return;

        const role = roles[0];

        const targetPath =
            role === "Admin" ? path.ADMIN.DASHBOARD :
                role === "Doctor" ? path.DOCTOR.TODAYAPPOINTMENT :
                    role === "Receptionist" ? path.RECEPTIONIST.MANAGEMENT :
                        role === "Patient" ? path.PATIENT.PROFILE.MANAGEMENT :
                            path.HOME;

        if (location.pathname !== targetPath) {
            navigate(targetPath, { replace: true });
        }
    }, [token, location.pathname]); 


    // Nếu đã login → không render gì cả (đang redirect)
    if (token && roles.length > 0) {
        return null;
    }

    // Nếu chưa login → cho vào trang login/register
    return <Outlet />;
};

export default RedirectIfLoggedIn;