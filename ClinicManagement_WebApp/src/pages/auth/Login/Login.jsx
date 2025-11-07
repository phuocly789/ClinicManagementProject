import React, { useState } from "react";
// Import thêm Button, InputGroup, Form từ react-bootstrap
import { Spinner, InputGroup, Button, Form } from "react-bootstrap";
import authService from "../../../services/authService";
import { useNavigate } from "react-router-dom";
import CustomToast from "../../../Components/CustomToast/CustomToast";
import { path } from "../../../utils/constant";

const LoginPage = () => {
  const [formData, setFormData] = useState({
    username: "",
    password: "",
    role: "", // Khởi tạo là chuỗi rỗng
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

    // ====== Kiểm tra role ======
    if (!formData.role) {
      newErrors.role = "Vui lòng chọn vai trò của bạn";
    }

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

    setLoading(true); // Bắt đầu loading

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
          else if (role === "Doctor") navigate("/doctor/today-appointment");
          else if (role === "Receptionist") navigate("/receptionist/appointment-management");
          else navigate("/");
        }, 1000);
      }
    } catch (error) {
      console.error(error);
      const message =
        error.response?.data?.message ||
        "Đã xảy ra lỗi. Vui lòng kiểm tra lại thông tin đăng nhập.";
      showToast("error", message);
    } finally {
      setLoading(false); // Dừng loading
    }
  };

  return (
    <>
      <div className="container-fluid bg-light min-vh-100 d-flex justify-content-center align-items-center">
        <div className="row w-100 justify-content-center">
          <div className="col-11 col-sm-8 col-md-6 col-lg-5 col-xl-4">
            <div
              className="card shadow-lg p-4 p-sm-5" // Tăng padding
              style={{ borderRadius: "16px" }}
            >
              <div className="text-center mb-3">
                {/* Thêm Icon Logo */}
                <i
                  className="bi bi-hospital text-primary"
                  style={{ fontSize: "3.5rem" }}
                ></i>
              </div>
              <h1 className="fs-2 text-center fw-bold text-primary mb-2">
                Hệ Thống Quản Lý Phòng Khám
              </h1>
              <p className="text-center text-muted mb-4 fs-6">
                Vui lòng đăng nhập để tiếp tục
              </p>

              <Form onSubmit={handleSubmit}>
                {/* Role selection (Nâng cấp) */}
                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    Chọn vai trò
                  </label>
                  {/* Sử dụng btn-group để nhóm các nút */}
                  <div
                    className="btn-group w-100"
                    role="group"
                    aria-label="Role selection"
                  >
                    {/* Cập nhật vai trò: Admin, Doctor, Technician */}
                    {["Patient","Admin", "Doctor", "Receptionist"].map((role) => (
                      <React.Fragment key={role}>
                        <input
                          type="radio"
                          className="btn-check"
                          name="role"
                          id={`role-${role}`}
                          autoComplete="off"
                          value={role}
                          checked={formData.role === role}
                          onChange={handleChange}
                        />
                        <label
                          className="btn btn-outline-primary"
                          htmlFor={`role-${role}`}
                        >
                          {role}
                        </label>
                      </React.Fragment>
                    ))}
                  </div>
                  {errors.role && (
                    <div className="text-danger mt-1" style={{ fontSize: "0.875em" }}>
                      {errors.role}
                    </div>
                  )}
                </div>

                {/* Username (Nâng cấp) */}
                <div className="mb-3">
                  <label htmlFor="username" className="form-label fw-semibold">
                    Tên đăng nhập
                  </label>
                  <InputGroup hasValidation>
                    <InputGroup.Text id="username-icon">
                      <i className="bi bi-person-fill"></i>
                    </InputGroup.Text>
                    <Form.Control
                      type="text"
                      name="username"
                      id="username"
                      placeholder="Nhập email hoặc số điện thoại"
                      value={formData.username}
                      onChange={handleChange}
                      isInvalid={!!errors.username}
                      aria-describedby="username-icon"
                    />
                    <Form.Control.Feedback type="invalid">
                      {errors.username}
                    </Form.Control.Feedback>
                  </InputGroup>
                </div>

                {/* Password (Nâng cấp) */}
                <div className="mb-4">
                  <label htmlFor="password" className="form-label fw-semibold">
                    Mật khẩu
                  </label>
                  <InputGroup hasValidation>
                    <InputGroup.Text id="password-icon">
                      <i className="bi bi-lock-fill"></i>
                    </InputGroup.Text>
                    <Form.Control
                      type={showPassword ? "text" : "password"} // Kích hoạt show/hide
                      name="password"
                      id="password"
                      placeholder="Nhập mật khẩu"
                      value={formData.password}
                      onChange={handleChange}
                      isInvalid={!!errors.password}
                      aria-describedby="password-icon"
                    />
                    {/* Nút Show/Hide Password */}
                    <Button
                      variant="outline-secondary"
                      onClick={() => setShowPassword(!showPassword)}
                    >
                      <i
                        className={
                          showPassword ? "bi bi-eye-slash" : "bi bi-eye"
                        }
                      ></i>
                    </Button>
                    <Form.Control.Feedback type="invalid">
                      {errors.password}
                    </Form.Control.Feedback>
                  </InputGroup>
                </div>

                <button
                  type="submit"
                  className="btn btn-primary btn-lg w-100 mb-3 fw-semibold"
                  disabled={loading}
                >
                  {loading ? (
                    <>
                      <Spinner
                        animation="border"
                        size="sm"
                        className="me-2"
                      />
                      Đang xử lý...
                    </>
                  ) : (
                    "Đăng Nhập"
                  )}
                </button>
              </Form>

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