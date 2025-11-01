import { Navigate, Outlet } from "react-router-dom";
import authService from "../services/authService";

const PrivateRoute = ({ allowedRoles }) => {
    const roles = authService.getRoles();
    const token = authService.getToken();

    if (!token) return <Navigate to="/login" replace />;

    if (allowedRoles && !allowedRoles.some((r) => roles.includes(r))) {
        return <Navigate to="/" replace />;
    }

    return <Outlet />;
};

export default PrivateRoute;
