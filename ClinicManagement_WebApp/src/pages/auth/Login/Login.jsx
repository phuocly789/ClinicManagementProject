import React, { useState } from "react";
import { Spinner } from "react-bootstrap";
import authService from "../../../services/authService";
import { useNavigate } from "react-router-dom";
import CustomToast from "../../../Components/CustomToast/CustomToast";
import { path } from "../../../utils/constant";
const LoginPage = () => {
  const [formData, setFormData] = useState({
    username: "",
    password: "",
    role: "",
  });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const [toast, setToast] = useState(null);
  const [showPassword, setShowPassword] = useState(false);

  const showToast = (type, message) => {
    setToast({ type, message });
  };

  // Xử lý thay đổi input
  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
    // reset lỗi khi user nhập lại
    setErrors({
      ...errors,
      [e.target.name]: "",
    });
  };

  // Validate cơ bản
  const validateForm = () => {
    const newErrors = {};
    const htmlTagRegex = /<[^>]*>/; // Regex phát hiện HTML tag

    // ====== Kiểm tra username ======
    if (!formData.username.trim()) {
      newErrors.username = "Tên đăng nhập không được để trống";
    } else if (formData.username.length < 6) {
      newErrors.username = "Tài khoản không được nhỏ hơn 6 ký tự";
    } else if (formData.username.length > 255) {
      newErrors.username = "Tài khoản không được lớn hơn 255 ký tự";
    } else if (htmlTagRegex.test(formData.username)) {
      newErrors.username = "Tài khoản không được chứa mã HTML";
    }

    // ====== Kiểm tra password ======
    if (!formData.password.trim()) {
      newErrors.password = "Mật khẩu không được để trống";
    } else if (formData.password.length < 6) {
      newErrors.password = "Mật khẩu không được nhỏ hơn 6 ký tự";
    } else if (formData.password.length > 255) {
      newErrors.password = "Mật khẩu không được lớn hơn 255 ký tự";
    } else if (htmlTagRegex.test(formData.password)) {
      newErrors.password = "Mật khẩu không được chứa mã HTML";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0; // true nếu không có lỗi
  };

  // Submit form
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;
    try {
      const res = await authService.handleLogin({
        emailOrPhone: formData.username,
        password: formData.password,
        role: formData.role,
      });
      if (res?.token) {
        showToast("success", "Đăng nhập thành công");

        // Lấy role của user
        const role = res.roles[0];

        setTimeout(() => {
          if (role === "Admin") navigate("/admin/dashboard");
          else if (role === "Doctor") navigate("/doctor/dashboard");
          else if (role === "Technician") navigate("/technician");
          else navigate("/");
        }, 1000);
      }
      
    } catch (error) {
      console.error(error);
      const message =
        error.response?.data?.message ||
        "Đã xảy ra lỗi ở phía server. Vui lòng đăng nhập lại";
      showToast("error", message);
    }
  };

  return (
    <>
      <div className="container-fluid bg-light min-vh-100 d-flex justify-content-center align-items-center">
        <div className="row w-100 justify-content-center">
          <div className="col-11 col-sm-8 col-md-6 col-lg-5 col-xl-4">
            <div
              className="card shadow-lg p-4 p-sm-6"
              style={{ borderRadius: "16px" }}
            >
              <h1 className="fs-2 text-center fw-bold text-primary mb-3">
                Hệ Thống Quản Lý Phòng Khám
              </h1>
              <p className="text-center text-muted mb-4 fs-6">
                Vui lòng đăng nhập để tiếp tục
              </p>

              <form onSubmit={handleSubmit}>
                {/* Role selection */}
                <div className="mb-3">
                  <label className="form-label fw-semibold">Chọn vai trò</label>
                  <div className="d-flex gap-4">
                    <div>
                      <input
                        type="radio"
                        id="admin"
                        name="role"
                        value="Admin"
                        checked={formData.role === "Admin"}
                        onChange={handleChange}
                      />
                      <label htmlFor="admin" className="ms-2">Admin</label>
                    </div>

                    <div>
                      <input
                        type="radio"
                        id="doctor"
                        name="role"
                        value="Doctor"
                        checked={formData.role === "Doctor"}
                        onChange={handleChange}
                      />
                      <label htmlFor="doctor" className="ms-2">Doctor</label>
                    </div>

                    <div>
                      <input
                        type="radio"
                        id="patient"
                        name="role"
                        value="Patient"
                        checked={formData.role === "Patient"}
                        onChange={handleChange}
                      />
                      <label htmlFor="patient" className="ms-2">Patient</label>
                    </div>
                  </div>
                </div>

                {/* Username */}
                <div className="mb-4">
                  <label htmlFor="username" className="form-label fw-semibold">
                    Tên đăng nhập
                  </label>
                  <input
                    type="text"
                    name="username"
                    id="username"
                    className={`form-control form-control-lg ${
                      errors.username ? "is-invalid" : ""
                    }`}
                    placeholder="Nhập email hoặc số điện thoại"
                    value={formData.username}
                    onChange={handleChange}
                  />
                  {errors.username && (
                    <div className="invalid-feedback">{errors.username}</div>
                  )}
                </div>

                {/* Password */}
                <div className="mb-4">
                  <label htmlFor="password" className="form-label fw-semibold">
                    Mật khẩu
                  </label>
                  <input
                    type="password"
                    name="password"
                    id="password"
                    className={`form-control form-control-lg ${
                      errors.password ? "is-invalid" : ""
                    }`}
                    placeholder="Nhập mật khẩu"
                    value={formData.password}
                    onChange={handleChange}
                  />
                  {errors.password && (
                    <div className="invalid-feedback">{errors.password}</div>
                  )}
                </div>

                <button
                  type="submit"
                  className="btn btn-primary btn-lg w-100 mb-3 fw-semibold"
                  disabled={loading}
                >
                  {loading ? (
                    <>
                      <Spinner animation="border" size="sm" className="me-2" />
                      Đang xử lý...
                    </>
                  ) : (
                    "Đăng Nhập"
                  )}
                </button>
              </form>

              <div className="text-center mt-3">
                <a href="#" className="text-decoration-none">
                  Quên mật khẩu?
                </a>
                <span className="mx-2 text-muted">|</span>
                <a href={path.REGISTER} className="text-decoration-none">
                  Đăng ký tài khoản mới
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
      {toast && (
        <CustomToast
          type={toast.type}
          message={toast.message}
          onClose={() => setToast(null)}
        />
      )}
    </>
  );
};

export default LoginPage;
