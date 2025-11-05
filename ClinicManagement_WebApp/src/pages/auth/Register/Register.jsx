import React, { useState } from "react";
// Import thêm Form, InputGroup, Button từ react-bootstrap
import { Spinner, Form, InputGroup, Button } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import CustomToast from "../../../Components/CustomToast/CustomToast";
import { path } from "../../../utils/constant";
import authService from "../../../services/authService";

const Register = () => {
  const [form, setForm] = useState({
    fullName: "",
    email: "",
    phoneNumber: "",
    address: "",
    password: "",
    confirmPassword: "",
    gender: "",
    dateOfBirth: "",
  });

  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState(null);
  const navigate = useNavigate();

  // Thêm state cho show/hide password
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const showToast = (type, message) => {
    setToast({ type, message });
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm({ ...form, [name]: value });
    setErrors({ ...errors, [name]: "" }); // reset lỗi khi user nhập lại
  };

  // ✅ Validate toàn bộ (Giữ nguyên logic của bạn vì nó đã rất tốt)
  const validate = () => {
    const temp = {};
    const htmlRegex = /<[^>]*>/g;
    const specialCharRegex = /[!@#$%^&*(),.?":{}|<>]/;
    const now = new Date();
    const birthdayDate = form.dateOfBirth ? new Date(form.dateOfBirth) : null;

    // Họ tên
    if (!form.fullName) temp.fullName = "Họ tên không được để trống";
    else if (htmlRegex.test(form.fullName))
      temp.fullName = "Vui lòng không nhập mã HTML";
    else if (specialCharRegex.test(form.fullName))
      temp.fullName = "Họ tên không được chứa ký tự đặc biệt";
    else if (form.fullName.length > 255)
      temp.fullName = "Họ tên không được quá 255 ký tự";

    // Email
    if (!form.email) temp.email = "Email không được để trống";
    else if (htmlRegex.test(form.email))
      temp.email = "Vui lòng không nhập mã HTML";
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      temp.email = "Email không hợp lệ";
    else if (form.email.length > 255)
      temp.email = "Email không được quá 255 ký tự";

    // Số điện thoại
    if (!form.phoneNumber) temp.phoneNumber = "Số điện thoại không được để trống";
    else if (!/^0\d{9,10}$/.test(form.phoneNumber))
      temp.phoneNumber = "Số điện thoại không hợp lệ";
    else if (form.phoneNumber.length > 11)
      temp.phoneNumber = "Số điện thoại không được quá 11 số";

    // Address
    if (!form.address) temp.address = "Địa chỉ không được để trống";
    else if (htmlRegex.test(form.address))
      temp.address = "Vui lòng không nhập mã HTML";
    else if (form.address.length > 500)
      temp.address = "Địa chỉ không được quá 500 ký tự";

    // Password
    if (!form.password) temp.password = "Mật khẩu không được để trống";
    else if (form.password.length < 6)
      temp.password = "Mật khẩu phải có ít nhất 6 ký tự"; // Thêm validate độ dài
    else if (htmlRegex.test(form.password))
      temp.password = "Vui lòng không nhập mã HTML";
    else if (form.password.length > 255)
      temp.password = "Mật khẩu không được quá 255 ký tự";

    // Confirm Password
    if (!form.confirmPassword)
      temp.confirmPassword = "Xác nhận mật khẩu không được để trống";
    else if (form.confirmPassword !== form.password)
      temp.confirmPassword = "Mật khẩu không khớp";

    // Gender
    if (!form.gender) temp.gender = "Giới tính không được để trống";

    // Birthday
    if (!form.dateOfBirth)
      temp.dateOfBirth = "Ngày tháng năm sinh không được để trống";
    else if (birthdayDate > now)
      temp.dateOfBirth = "Không được chọn ngày tháng năm sinh trong tương lai";

    setErrors(temp);
    return Object.keys(temp).length === 0;
  };

  // ✅ Submit (Giữ nguyên)
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;

    setLoading(true);
    try {
      const payload = { ...form };
      const res = await authService.handleRegister(payload);

      if (res?.status === "Success") {
        showToast(
          "success",
          res.message || "Đăng ký thành công. Vui lòng kiểm tra email để xác thực."
        );
        setTimeout(() => navigate(path.LOGIN), 1500); // Tăng thời gian chờ
      } else {
        showToast("error", res?.message || "Đăng ký thất bại!");
      }
    } catch (err) {
      showToast("error", err.response?.data?.message || "Lỗi máy chủ. Vui lòng thử lại.");
    } finally {
      setLoading(false);
    }
  };

  // ✅ Get Location (Giữ nguyên)
  const handleGetLocation = () => {
    if (!navigator.geolocation) {
      showToast("error", "Trình duyệt không hỗ trợ định vị.");
      return;
    }

    showToast("info", "Đang xác định vị trí...");

    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const { latitude, longitude } = position.coords;

        try {
          const response = await fetch(
            `https://nominatim.openstreetmap.org/reverse?lat=${latitude}&lon=${longitude}&format=json&accept-language=vi`
          );
          const data = await response.json();

          if (data?.display_name) {
            setForm((prev) => ({ ...prev, address: data.display_name }));
            showToast("success", "Đã lấy vị trí thành công");
          } else {
            showToast("error", "Không tìm được địa chỉ từ vị trí GPS.");
          }
        } catch (error) {
          showToast("error", "Lỗi khi lấy địa chỉ từ GPS.");
        }
      },
      () => {
        showToast("error", "Không thể truy cập GPS. Hãy bật định vị.");
      }
    );
  };

  return (
    <>
      <div className="container-fluid bg-light min-vh-100 d-flex justify-content-center align-items-center py-4">
        <div className="row w-100 justify-content-center">
          <div className="col-11 col-sm-10 col-md-9 col-lg-8 col-xl-7">
            <div
              className="card shadow-lg p-4 p-md-5" // Tăng padding
              style={{ borderRadius: "16px" }}
            >
              <div className="text-center mb-3">
                <i
                  className="bi bi-hospital text-primary"
                  style={{ fontSize: "3.5rem" }}
                ></i>
              </div>
              <h1 className="fs-3 text-center fw-bold text-primary mb-2">
                Đăng ký tài khoản Bệnh nhân
              </h1>
              <p className="text-center text-muted mb-4 fs-6">
                Vui lòng điền thông tin bên dưới để tạo tài khoản
              </p>

              {/* Thay thế <form> bằng <Form> của react-bootstrap */}
              <Form onSubmit={handleSubmit}>
                <div className="row">
                  {/* Cột trái */}
                  <div className="col-md-6">
                    {/* Họ và tên */}
                    <Form.Group className="mb-3" controlId="fullName">
                      <Form.Label className="fw-semibold">Họ và tên</Form.Label>
                      <InputGroup hasValidation>
                        <InputGroup.Text>
                          <i className="bi bi-person"></i>
                        </InputGroup.Text>
                        <Form.Control
                          type="text"
                          name="fullName"
                          placeholder="Nhập họ và tên"
                          value={form.fullName}
                          onChange={handleChange}
                          isInvalid={!!errors.fullName}
                        />
                        <Form.Control.Feedback type="invalid">
                          {errors.fullName}
                        </Form.Control.Feedback>
                      </InputGroup>
                    </Form.Group>

                    {/* Email */}
                    <Form.Group className="mb-3" controlId="email">
                      <Form.Label className="fw-semibold">Email</Form.Label>
                      <InputGroup hasValidation>
                        <InputGroup.Text>
                          <i className="bi bi-envelope"></i>
                        </InputGroup.Text>
                        <Form.Control
                          type="email"
                          name="email"
                          placeholder="Nhập email"
                          value={form.email}
                          onChange={handleChange}
                          isInvalid={!!errors.email}
                        />
                        <Form.Control.Feedback type="invalid">
                          {errors.email}
                        </Form.Control.Feedback>
                      </InputGroup>
                    </Form.Group>

                    {/* Số điện thoại */}
                    <Form.Group className="mb-3" controlId="phoneNumber">
                      <Form.Label className="fw-semibold">Số điện thoại</Form.Label>
                      <InputGroup hasValidation>
                        <InputGroup.Text>
                          <i className="bi bi-phone"></i>
                        </InputGroup.Text>
                        <Form.Control
                          type="text"
                          name="phoneNumber"
                          placeholder="Nhập số điện thoại"
                          value={form.phoneNumber}
                          onChange={handleChange}
                          isInvalid={!!errors.phoneNumber}
                        />
                        <Form.Control.Feedback type="invalid">
                          {errors.phoneNumber}
                        </Form.Control.Feedback>
                      </InputGroup>
                    </Form.Group>

                    {/* Địa chỉ */}
                    <Form.Group className="mb-3" controlId="address">
                      <Form.Label className="fw-semibold">Địa chỉ</Form.Label>
                      <InputGroup hasValidation>
                        <Form.Control
                          type="text"
                          name="address"
                          placeholder="Nhập địa chỉ"
                          value={form.address}
                          onChange={handleChange}
                          isInvalid={!!errors.address}
                        />
                        {/* Nút lấy vị trí bằng icon */}
                        <Button
                          variant="outline-secondary"
                          onClick={handleGetLocation}
                          title="Lấy vị trí hiện tại"
                        >
                          <i className="bi bi-geo-alt-fill"></i>
                        </Button>
                        <Form.Control.Feedback type="invalid">
                          {errors.address}
                        </Form.Control.Feedback>
                      </InputGroup>
                    </Form.Group>
                  </div>

                  {/* Cột phải */}
                  <div className="col-md-6">
                    {/* Mật khẩu */}
                    <Form.Group className="mb-3" controlId="password">
                      <Form.Label className="fw-semibold">Mật khẩu</Form.Label>
                      <InputGroup hasValidation>
                        <InputGroup.Text>
                          <i className="bi bi-lock"></i>
                        </InputGroup.Text>
                        <Form.Control
                          type={showPassword ? "text" : "password"}
                          name="password"
                          placeholder="Nhập mật khẩu"
                          value={form.password}
                          onChange={handleChange}
                          isInvalid={!!errors.password}
                        />
                        <Button
                          variant="outline-secondary"
                          onClick={() => setShowPassword(!showPassword)}
                        >
                          <i className={showPassword ? "bi bi-eye-slash" : "bi bi-eye"}></i>
                        </Button>
                        <Form.Control.Feedback type="invalid">
                          {errors.password}
                        </Form.Control.Feedback>
                      </InputGroup>
                    </Form.Group>

                    {/* Xác nhận mật khẩu */}
                    <Form.Group className="mb-3" controlId="confirmPassword">
                      <Form.Label className="fw-semibold">Xác nhận mật khẩu</Form.Label>
                      <InputGroup hasValidation>
                        <InputGroup.Text>
                          <i className="bi bi-lock-fill"></i>
                        </InputGroup.Text>
                        <Form.Control
                          type={showConfirmPassword ? "text" : "password"}
                          name="confirmPassword"
                          placeholder="Nhập lại mật khẩu"
                          value={form.confirmPassword}
                          onChange={handleChange}
                          isInvalid={!!errors.confirmPassword}
                        />
                        <Button
                          variant="outline-secondary"
                          onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        >
                          <i className={showConfirmPassword ? "bi bi-eye-slash" : "bi bi-eye"}></i>
                        </Button>
                        <Form.Control.Feedback type="invalid">
                          {errors.confirmPassword}
                        </Form.Control.Feedback>
                      </InputGroup>
                    </Form.Group>

                    {/* Giới tính */}
                    <Form.Group className="mb-3" controlId="gender">
                      <Form.Label className="fw-semibold">Giới tính</Form.Label>
                      <InputGroup hasValidation>
                        <InputGroup.Text>
                          <i className="bi bi-gender-ambiguous"></i>
                        </InputGroup.Text>
                        <Form.Select
                          name="gender"
                          value={form.gender}
                          onChange={handleChange}
                          isInvalid={!!errors.gender}
                        >
                          <option value="">-- Chọn giới tính --</option>
                          <option value="Nam">Nam</option>
                          <option value="Nữ">Nữ</option>
                          <option value="Khác">Khác</option>
                        </Form.Select>
                        <Form.Control.Feedback type="invalid">
                          {errors.gender}
                        </Form.Control.Feedback>
                      </InputGroup>
                    </Form.Group>

                    {/* Ngày sinh */}
                    <Form.Group className="mb-3" controlId="dateOfBirth">
                      <Form.Label className="fw-semibold">Ngày tháng năm sinh</Form.Label>
                      <InputGroup hasValidation>
                        <InputGroup.Text>
                          <i className="bi bi-calendar-event"></i>
                        </InputGroup.Text>
                        <Form.Control
                          type="date"
                          name="dateOfBirth"
                          value={form.dateOfBirth}
                          onChange={handleChange}
                          isInvalid={!!errors.dateOfBirth}
                        />
                        <Form.Control.Feedback type="invalid">
                          {errors.dateOfBirth}
                        </Form.Control.Feedback>
                      </InputGroup>
                    </Form.Group>
                  </div>
                </div>

                <button
                  type="submit"
                  className="btn btn-primary btn-lg w-100 fw-semibold mt-3"
                  disabled={loading}
                >
                  {loading ? (
                    <>
                      <Spinner animation="border" size="sm" className="me-2" />
                      Đang xử lý...
                    </>
                  ) : (
                    "Đăng ký"
                  )}
                </button>
              </Form>

              <div className="text-center mt-4"> {/* Tăng margin top */}
                <span>Bạn đã có tài khoản? </span>
                <a
                  href={path.LOGIN}
                  className="text-decoration-none text-primary fw-semibold"
                >
                  Đăng nhập
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

export default Register;