import instance from "../axios";

const API_URL = "http://localhost:5066/api/Auth";

const handleLogin = async (data) => {
  const res = await instance.post(`${API_URL}/UserLogin`, data);
  if (res?.token) {
    localStorage.setItem("token", res.token);
    localStorage.setItem("roles", JSON.stringify(res.roles));
  }
  return res;
};

const handleRegister = async (data) => {
  const res = await instance.post(`${API_URL}/PatientRegister`, data);
  return res;
}

const logout = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("roles");
};

const getToken = () => localStorage.getItem("token");
const getRoles = () => JSON.parse(localStorage.getItem("roles") || "[]");

export default { handleLogin, logout, getToken, getRoles,handleRegister };
