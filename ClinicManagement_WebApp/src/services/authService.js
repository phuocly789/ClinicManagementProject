import instance from "../axios";
import { jwtDecode } from "jwt-decode";

const API_URL = "https://clinicapi.lmp.id.vn/api/Auth";

// === HÀM CHECK TOKEN HẾT HẠN (SIÊU ỔN ĐỊNH) ===
const isTokenExpired = () => {
  const token = localStorage.getItem("token");
  if (!token) return true;

  try {
    const decoded = jwtDecode(token);
    if (!decoded.exp) return false;
    return decoded.exp < Date.now() / 1000;
  } catch (err) {
    console.warn("Token lỗi định dạng → tự động logout", err);
    logout();
    return true;
  }
};

// === GET TOKEN AN TOÀN ===
const getToken = () => {
  return isTokenExpired() ? (null) : localStorage.getItem("token");
};

// === GET ROLES AN TOÀN ===
const getRoles = () => {
  return isTokenExpired() ? ([]) : JSON.parse(localStorage.getItem("roles") || "[]");
};

// === GET USERNAME / FULLNAME (KHÔNG LỖI) ===
const getUsernameFromToken = () => {
  const token = getToken();
  if (!token) return null;
  try {
    const decoded = jwtDecode(token);
    return (
      decoded.username ||
      decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
      decoded.sub ||
      null
    );
  } catch {
    return null;
  }
};

const getFullNameFromToken = () => {
  const token = getToken();
  if (!token) return null;
  try {
    const decoded = jwtDecode(token);
    return (
      decoded.fullname ||
      decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/fullname"] ||
      decoded.name ||
      null
    );
  } catch {
    return null;
  }
};

// === LOGIN / REGISTER / LOGOUT ===
const handleLogin = async (data) => {
  const res = await instance.post(`${API_URL}/UserLogin`, data);
  if (res?.token) {
    localStorage.setItem("token", res.token);
    localStorage.setItem("roles", JSON.stringify(res.roles || []));
  }
  return res;
};

const handleRegister = async (data) => {
  return await instance.post(`${API_URL}/PatientRegister`, data);
};

const logout = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("roles");
  // Optional: đẩy về login
  window.location.href = "/login";
};

// === EXPORT ===
export default {
  handleLogin,
  handleRegister,
  logout,
  getToken,
  getRoles,
  getUsernameFromToken,
  getFullNameFromToken,
  isTokenExpired,
};