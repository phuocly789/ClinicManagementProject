import React, { useState } from "react";
import { Spinner } from "react-bootstrap";
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

  const showToast = (type, message) => {
    setToast({ type, message });
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm({ ...form, [name]: value });
    setErrors({ ...errors, [name]: "" }); // reset lỗi khi user nhập lại
  };

  // ✅ Validate toàn bộ
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

  // ✅ Submit
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;

    setLoading(true);
    try {

      const payload = {
        fullName: form.fullName,
        email: form.email,
        phoneNumber: form.phoneNumber,
        address: form.address,
        password: form.password,
        confirmPassword: form.confirmPassword,
        gender: form.gender,
        dateOfBirth: form.dateOfBirth,
      };

      const res = await authService.handleRegister(payload);

      if (res?.status === "Success") {
        showToast(
          "success",
          res.message || "Đăng ký tài khoản thành công. Vui lòng nhập mã OTP để xác thực tài khoản."
        );
        // Chuyển hướng sau
        setTimeout(() => navigate(path.LOGIN), 1200);


      } else {
        showToast("error", res?.message || "Đăng ký thất bại!");
      }
    } catch (err) {
      showToast("error", err.response?.data?.message || "Lỗi máy chủ. Vui lòng thử lại.");
    } finally {
      setLoading(false);
    }
  };
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
              className="card shadow-lg p-4"
              style={{ borderRadius: "16px" }}
            >
              <h1 className="fs-3 text-center fw-bold text-primary mb-2">
                Đăng ký tài khoản Bệnh nhân
              </h1>
              <p className="text-center text-muted mb-4 fs-6">
                Vui lòng điền thông tin bên dưới để tạo tài khoản
              </p>

              <form onSubmit={handleSubmit}>
                <div className="row">
                  {/* Cột trái */}
                  <div className="col-md-6">
                    {[
                      { label: "Họ và tên", name: "fullName", type: "text" },
                      { label: "Email", name: "email", type: "email" },
                      { label: "Số điện thoại", name: "phoneNumber", type: "text" },
                    ].map((input) => (
                      <div className="mb-3" key={input.name}>
                        <label className="form-label fw-semibold">
                          {input.label}
                        </label>
                        <input
                          type={input.type}
                          name={input.name}
                          className={`form-control ${errors[input.name] ? "is-invalid" : ""
                            }`}
                          placeholder={`Nhập ${input.label.toLowerCase()}`}
                          value={form[input.name]}
                          onChange={handleChange}
                        />
                        {errors[input.name] && (
                          <div className="invalid-feedback">
                            {errors[input.name]}
                          </div>
                        )}
                      </div>
                    ))}
                    {/* Địa chỉ */}
                    <div className="mb-3">
                      <label className="form-label fw-semibold">Địa chỉ</label>
                      <div className="input-group">
                        <input
                          type="text"
                          name="address"
                          className={`form-control ${errors.address ? "is-invalid" : ""}`}
                          placeholder="Nhập địa chỉ hoặc bấm nút lấy vị trí"
                          value={form.address}
                          onChange={handleChange}
                        />
                        <button
                          type="button"
                          className="btn btn-outline-secondary"
                          onClick={handleGetLocation}
                        >
                          Lấy vị trí
                        </button>
                      </div>
                      {errors.address && (
                        <div className="invalid-feedback d-block">{errors.address}</div>
                      )}
                    </div>
                  </div>
                {/* Cột phải */}
                <div className="col-md-6">
                  {/* Mật khẩu */}
                  <div className="mb-3">
                    <label className="form-label fw-semibold">Mật khẩu</label>
                    <input
                      type="password"
                      name="password"
                      className={`form-control ${errors.password ? "is-invalid" : ""
                        }`}
                      placeholder="Nhập mật khẩu"
                      value={form.password}
                      onChange={handleChange}
                    />
                    {errors.password && (
                      <div className="invalid-feedback">
                        {errors.password}
                      </div>
                    )}
                  </div>

                  {/* Xác nhận mật khẩu */}
                  <div className="mb-3">
                    <label className="form-label fw-semibold">
                      Xác nhận mật khẩu
                    </label>
                    <input
                      type="password"
                      name="confirmPassword"
                      className={`form-control ${errors.confirmPassword ? "is-invalid" : ""
                        }`}
                      placeholder="Nhập lại mật khẩu"
                      value={form.confirmPassword}
                      onChange={handleChange}
                    />
                    {errors.confirmPassword && (
                      <div className="invalid-feedback">
                        {errors.confirmPassword}
                      </div>
                    )}
                  </div>

                  {/* Giới tính */}
                  <div className="mb-3">
                    <label className="form-label fw-semibold">
                      Giới tính
                    </label>
                    <select
                      name="gender"
                      className={`form-select ${errors.gender ? "is-invalid" : ""
                        }`}
                      value={form.gender}
                      onChange={handleChange}
                    >
                      <option value="">-- Chọn giới tính --</option>
                      <option value="Nam">Nam</option>
                      <option value="Nữ">Nữ</option>
                      <option value="Khác">Khác</option>
                    </select>
                    {errors.gender && (
                      <div className="invalid-feedback">{errors.gender}</div>
                    )}
                  </div>

                  {/* Ngày sinh */}
                  <div className="mb-3">
                    <label className="form-label fw-semibold">
                      Ngày tháng năm sinh
                    </label>
                    <input
                      type="date"
                      name="dateOfBirth"
                      className={`form-control ${errors.dateOfBirth ? "is-invalid" : ""
                        }`}
                      value={form.dateOfBirth}
                      onChange={handleChange}
                    />
                    {errors.dateOfBirth && (
                      <div className="invalid-feedback">
                        {errors.dateOfBirth}
                      </div>
                    )}
                  </div>
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
          </form>

          <div className="text-center mt-3">
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
    </div >
      </div >

  { toast && (
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
