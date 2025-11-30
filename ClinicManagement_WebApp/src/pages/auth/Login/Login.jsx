import React, { useState } from "react";
import { Spinner, InputGroup, Button, Form, Modal } from "react-bootstrap";
import authService from "../../../services/authService";
import { useNavigate } from "react-router-dom";
import CustomToast from "../../../Components/CustomToast/CustomToast";
import { path } from "../../../utils/constant";
import instance from "../../../axios"; // Import instance để gọi API

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

  // State cho quên mật khẩu
  const [showForgotPasswordModal, setShowForgotPasswordModal] = useState(false);
  const [forgotPasswordData, setForgotPasswordData] = useState({
    email: ""
  });
  const [forgotPasswordLoading, setForgotPasswordLoading] = useState(false);
  const [forgotPasswordErrors, setForgotPasswordErrors] = useState({});

  const showToast = (type, message) => {
    setToast({ type, message });
  };

  // Xử lý thay đổi input
  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
    setErrors({
      ...errors,
      [e.target.name]: "",
    });
  };

  // Xử lý thay đổi input trong modal quên mật khẩu
  const handleForgotPasswordChange = (e) => {
    setForgotPasswordData({
      ...forgotPasswordData,
      [e.target.name]: e.target.value,
    });
    setForgotPasswordErrors({
      ...forgotPasswordErrors,
      [e.target.name]: "",
    });
  };

  // Validate cơ bản
  const validateForm = () => {
    const newErrors = {};
    const htmlTagRegex = /<[^>]*>/;

    if (!formData.role) {
      newErrors.role = "Vui lòng chọn vai trò của bạn";
    }

    if (!formData.username.trim()) {
      newErrors.username = "Tên đăng nhập không được để trống";
    } else if (formData.username.length < 6) {
      newErrors.username = "Tài khoản không được nhỏ hơn 6 ký tự";
    } else if (formData.username.length > 255) {
      newErrors.username = "Tài khoản không được lớn hơn 255 ký tự";
    } else if (htmlTagRegex.test(formData.username)) {
      newErrors.username = "Tài khoản không được chứa mã HTML";
    }

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
    return Object.keys(newErrors).length === 0;
  };

  // Validate form quên mật khẩu
  const validateForgotPasswordForm = () => {
    const newErrors = {};
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

   

    if (!forgotPasswordData.email.trim()) {
      newErrors.email = "Email không được để trống";
    } else if (!emailRegex.test(forgotPasswordData.email)) {
      newErrors.email = "Email không hợp lệ";
    }

    setForgotPasswordErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Submit form đăng nhập
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;

    setLoading(true);

    try {
      const res = await authService.handleLogin({
        emailOrPhone: formData.username,
        password: formData.password,
        role: formData.role,
      });
      if (res?.token) {
        showToast("success", "Đăng nhập thành công");

        const role = res.roles[0];
        setTimeout(() => {
          if (role === "Admin") navigate("/admin/dashboard");
          else if (role === "Doctor") navigate("/doctor/today-appointment");
          else if (role === "Receptionist") navigate("/receptionist/dashboard");
          else if (role === "Patient") navigate(path.PATIENT.PROFILE.MANAGEMENT);
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
      setLoading(false);
    }
  };

  // Xử lý quên mật khẩu
  const handleForgotPassword = async (e) => {
    e.preventDefault();
    if (!validateForgotPasswordForm()) return;

    setForgotPasswordLoading(true);
    try {
      // Gọi API reset password
      await instance.post("Auth/ResetPassword",{email: forgotPasswordData.email});

      showToast("success", "Mật khẩu mới đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.");
      setShowForgotPasswordModal(false);
      setForgotPasswordData({ email: ""});
    } catch (error) {
      console.error(error);
      const message = error.response?.data || "Có lỗi xảy ra khi gửi yêu cầu. Vui lòng thử lại.";
      showToast("error", message);
    } finally {
      setForgotPasswordLoading(false);
    }
  };

  // Mở modal quên mật khẩu
  const openForgotPasswordModal = () => {
    setForgotPasswordData({ email: ""});
    setForgotPasswordErrors({});
    setShowForgotPasswordModal(true);
  };

  // Đóng modal quên mật khẩu
  const closeForgotPasswordModal = () => {
    setShowForgotPasswordModal(false);
    setForgotPasswordData({ email: ""});
    setForgotPasswordErrors({});
  };

  return (
    <>
      <div className="container-fluid bg-light min-vh-100 d-flex justify-content-center align-items-center">
        <div className="row w-100 justify-content-center">
          <div className="col-11 col-sm-8 col-md-6 col-lg-5 col-xl-4">
            <div
              className="card shadow-lg p-4 p-sm-5"
              style={{ borderRadius: "16px" }}
            >
              <div className="text-center mb-3">
                <img src="/logo1.png" alt="logo" className="sidebar-logo" />
              </div>
              <h1 className="fs-2 text-center fw-bold text-primary mb-2">
                Hệ Thống Quản Lý Phòng Khám
              </h1>
              <p className="text-center text-muted mb-4 fs-6">
                Vui lòng đăng nhập để tiếp tục
              </p>

              <Form onSubmit={handleSubmit}>
                {/* Role selection */}
                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    Chọn vai trò
                  </label>
                  <div
                    className="btn-group w-100"
                    role="group"
                    aria-label="Role selection"
                  >
                    {["Patient", "Admin", "Doctor", "Receptionist"].map((role) => (
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

                {/* Username */}
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

                {/* Password */}
                <div className="mb-4">
                  <label htmlFor="password" className="form-label fw-semibold">
                    Mật khẩu
                  </label>
                  <InputGroup hasValidation>
                    <InputGroup.Text id="password-icon">
                      <i className="bi bi-lock-fill"></i>
                    </InputGroup.Text>
                    <Form.Control
                      type={showPassword ? "text" : "password"}
                      name="password"
                      id="password"
                      placeholder="Nhập mật khẩu"
                      value={formData.password}
                      onChange={handleChange}
                      isInvalid={!!errors.password}
                      aria-describedby="password-icon"
                    />
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
                <a
                  href="#"
                  className="text-decoration-none"
                  onClick={openForgotPasswordModal}
                >
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

      {/* Modal Quên Mật Khẩu */}
      <Modal show={showForgotPasswordModal} onHide={closeForgotPasswordModal} centered>
        <Modal.Header closeButton>
          <Modal.Title>
            <i className="bi bi-key me-2 text-warning"></i>
            Quên mật khẩu
          </Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleForgotPassword}>
          <Modal.Body>
            <p className="mb-4">
              Vui lòng nhập email của bạn để nhận mật khẩu mới.
            </p>


            {/* Email input */}
            <Form.Group className="mb-3">
              <Form.Label className="fw-semibold">
                Email đăng ký <span className="text-danger">*</span>
              </Form.Label>
              <InputGroup hasValidation>
                <InputGroup.Text>
                  <i className="bi bi-envelope"></i>
                </InputGroup.Text>
                <Form.Control
                  type="email"
                  name="email"
                  placeholder="Nhập email đã đăng ký tài khoản"
                  value={forgotPasswordData.email}
                  onChange={handleForgotPasswordChange}
                  isInvalid={!!forgotPasswordErrors.email}
                />
                <Form.Control.Feedback type="invalid">
                  {forgotPasswordErrors.email}
                </Form.Control.Feedback>
              </InputGroup>
              <Form.Text className="text-muted">
                Mật khẩu mới sẽ được gửi đến email này
              </Form.Text>
            </Form.Group>

            <div className="alert alert-info">
              <i className="bi bi-info-circle me-2"></i>
              Sau khi nhận được mật khẩu mới, vui lòng đăng nhập và đổi mật khẩu ngay để bảo mật tài khoản.
            </div>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={closeForgotPasswordModal}>
              Hủy
            </Button>
            <Button
              variant="warning"
              type="submit"
              disabled={forgotPasswordLoading}
            >
              {forgotPasswordLoading ? (
                <>
                  <Spinner animation="border" size="sm" className="me-2" />
                  Đang xử lý...
                </>
              ) : (
                <>
                  <i className="bi bi-send me-2"></i>
                  Gửi mật khẩu mới
                </>
              )}
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>

      {/* Toast */}
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