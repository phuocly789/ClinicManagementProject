// PatientProfile.jsx - HOÀN CHỈNH VỚI API MỚI
import React, { useCallback, useEffect, useState } from "react";
import instance from "../../axios";
import authService from "../../services/authService";
import OtpModal from "../../Components/Auth/OtpModal";
import bcrypt from "bcryptjs";
import Loading from "../../Components/Loading/Loading";
import CustomToast from "../../Components/CustomToast/CustomToast";
import { Button, Card, Form, Row, Col, InputGroup, Modal } from "react-bootstrap";

const PatientProfile = () => {
  const [profileData, setProfileData] = useState({
    username: "",
    fullName: "",
    email: "",
    phone: "",
    address: "",
    dateOfBirth: "",
    passwordHash: "",
    gender: "",
    mustChangePassword: false
  });

  const [passwordData, setPasswordData] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });

  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState(null);
  const [username, setUsername] = useState("");
  const [showOtpModalPassword, setShowOtpModalPassword] = useState(false);
  const [showOtpModalUpdate, setShowOtpModalUpdate] = useState(false);
  const [showOtpModalDeactivate, setShowOtpModalDeactivate] = useState(false);
  const [showOtpModalForgotPassword, setShowOtpModalForgotPassword] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [buttonSave, setButtonSave] = useState(false);

  // State cho modal
  const [showForgotPasswordModal, setShowForgotPasswordModal] = useState(false);
  const [showDeactivateModal, setShowDeactivateModal] = useState(false);
  const [deactivateReason, setDeactivateReason] = useState("");
  const [showForceChangePasswordModal, setShowForceChangePasswordModal] = useState(false);

  // Lấy username từ token
  useEffect(() => {
    const user = authService.getUsernameFromToken();
    setUsername(user);
  }, []);

  const validateProfileForm = () => {
    const newErrors = {};
    const htmlRegex = /<[^>]*>/g;
    const now = new Date();

    if (!profileData.fullName.trim())
      newErrors.fullName = "Họ tên không được để trống";
    else if (htmlRegex.test(profileData.fullName))
      newErrors.fullName = "Không được nhập mã HTML trong họ tên";

    if (!profileData.phone)
      newErrors.phone = "Số điện thoại không được để trống";
    else if (!/^0\d{9,10}$/.test(profileData.phone))
      newErrors.phone = "Số điện thoại không hợp lệ";

    if (!profileData.dateOfBirth)
      newErrors.dateOfBirth = "Ngày sinh không được để trống";
    else if (new Date(profileData.dateOfBirth) > now)
      newErrors.dateOfBirth = "Ngày sinh không được vượt quá ngày hiện tại";

    if (!profileData.address.trim())
      newErrors.address = "Địa chỉ không được để trống";
    else if (profileData.address.length > 500)
      newErrors.address = "Địa chỉ không được quá 500 ký tự";

    if (!profileData.gender)
      newErrors.gender = "Giới tính không được bỏ trống";

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Lấy thông tin hồ sơ
  const fetchProfile = useCallback(async () => {
    if (!username) return;
    setLoading(true);
    try {
      const res = await instance.get(`User/GetByUsername/${username}`);
      const user = res.data || res;

      setProfileData({
        username: user.username || "",
        fullName: user.fullName || "",
        email: user.email || "",
        phone: user.phone || "",
        address: user.address || "",
        gender: user.gender || "",
        dateOfBirth: user.dateOfBirth?.split("T")[0] || "",
        passwordHash: user.passwordHash || user.password || "",
        mustChangePassword: user.mustChangePassword || false
      });

      // Kiểm tra nếu phải đổi mật khẩu bắt buộc
      if (user.mustChangePassword) {
        setShowForceChangePasswordModal(true);
      }
    } catch (err) {
      setToast({ type: "error", message: "Không tải được hồ sơ!" });
    } finally {
      setLoading(false);
    }
  }, [username]);

  useEffect(() => {
    fetchProfile();
  }, [fetchProfile]);

  // === XỬ LÝ HỒ SƠ ===
  const handleProfileChange = (e) => {
    const { name, value } = e.target;
    setProfileData({ ...profileData, [name]: value });
    setButtonSave(true);
    setErrors({ ...errors, [name]: "" });
  };

  const handleConfirmUpdateProfile = async (e) => {
    e.preventDefault();
    if (!validateProfileForm()) return;
    setLoading(true);
    try {
      await instance.post("Auth/SendOTP", { email: profileData.email });
      setToast({ type: "success", message: "Mã OTP đã gửi đến email!" });
      setShowOtpModalUpdate(true);
    } catch (err) {
      setToast({ type: "error", message: err.response?.data || "Gửi OTP thất bại!" });
    } finally {
      setLoading(false);
    }
  };

  const handleSaveProfile = async () => {
    setLoading(true);
    try {
      const payload = {
        fullName: profileData.fullName,
        phone: profileData.phone,
        address: profileData.address,
        dateOfBirth: profileData.dateOfBirth,
        gender: profileData.gender,
      }
      await instance.put(`User/UpdateUser/${username}`, payload);
      setToast({ type: "success", message: "Cập nhật hồ sơ thành công!" });
      setShowOtpModalUpdate(false);
      setButtonSave(false);
      fetchProfile();
    } catch (err) {
      setToast({ type: "error", message: "Cập nhật thất bại!" });
    } finally {
      setLoading(false);
    }
  };

  // === XỬ LÝ ĐỔI MẬT KHẨU ===
  const handlePasswordChange = (e) => {
    const { name, value } = e.target;
    setPasswordData({ ...passwordData, [name]: value });
    setErrors({ ...errors, [name]: "" });
  };

  const verifyCurrentPassword = async (input) => {
    if (!profileData.passwordHash) return false;
    if (profileData.passwordHash === input) return true;
    return await bcrypt.compare(input, profileData.passwordHash);
  };

  const validatePasswordForm = async () => {
    const newErrors = {};

    if (!passwordData.currentPassword) newErrors.currentPassword = "Vui lòng nhập mật khẩu hiện tại";
    if (!passwordData.newPassword) newErrors.newPassword = "Vui lòng nhập mật khẩu mới";
    else if (passwordData.newPassword.length < 6) newErrors.newPassword = "Mật khẩu ít nhất 6 ký tự";
    else if (passwordData.newPassword === passwordData.currentPassword) newErrors.newPassword = "Mật khẩu mới phải khác mật khẩu cũ";

    if (!passwordData.confirmPassword) newErrors.confirmPassword = "Vui lòng xác nhận mật khẩu";
    else if (passwordData.newPassword !== passwordData.confirmPassword) newErrors.confirmPassword = "Mật khẩu không khớp";

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleConfirmChangePassword = async (e) => {
    e.preventDefault();
    if (!(await validatePasswordForm())) return;

    setLoading(true);
    const isCorrect = await verifyCurrentPassword(passwordData.currentPassword);

    if (!isCorrect) {
      setErrors({ currentPassword: "Mật khẩu hiện tại không đúng!" });
      setToast({ type: "error", message: "Mật khẩu hiện tại sai!" });
      setLoading(false);
      return;
    }

    try {
      await instance.post("Auth/SendOTP", { email: profileData.email });
      setToast({ type: "success", message: "Mã OTP đã gửi đến email!" });
      setShowOtpModalPassword(true);
    } catch (err) {
      setToast({ type: "error", message: err.response?.data || "Gửi OTP thất bại!" });
    } finally {
      setLoading(false);
    }
  };

  const handleChangePasswordAfterOtp = async () => {
    setLoading(true);
    try {
      await instance.put(`User/ChangePassword/${username}`, {
        currentPassword: passwordData.currentPassword,
        newPassword: passwordData.newPassword,
      });
      setToast({ type: "success", message: "Đổi mật khẩu thành công!" });
      setPasswordData({ currentPassword: "", newPassword: "", confirmPassword: "" });
      setShowOtpModalPassword(false);

      // Reset mustChangePassword sau khi đổi mật khẩu thành công
      setProfileData(prev => ({ ...prev, mustChangePassword: false }));
      setShowForceChangePasswordModal(false);

      fetchProfile();
    } catch (err) {
      setToast({ type: "error", message: err.response?.data?.message || "Đổi mật khẩu thất bại!" });
    } finally {
      setLoading(false);
    }
  };

  // === CHỨC NĂNG MỚI VỚI API ===

  // Quên mật khẩu - Gửi OTP trước
  const handleForgotPassword = async () => {
    setLoading(true);
    try {
      await instance.post("Auth/SendOTP", { email: profileData.email });
      setToast({ type: "success", message: "Mã OTP đã gửi đến email!" });
      setShowForgotPasswordModal(false);
      setShowOtpModalForgotPassword(true);
    } catch (err) {
      setToast({ type: "error", message: err.response?.data || "Gửi OTP thất bại!" });
    } finally {
      setLoading(false);
    }
  };

  // Xác nhận quên mật khẩu sau OTP
  const handleForgotPasswordAfterOtp = async () => {
    setLoading(true);
    try {
      await instance.post("Auth/ResetPassword", { email: profileData.email });
      setToast({
        type: "success",
        message: "Mật khẩu mới đã được gửi đến email của bạn. Vui lòng kiểm tra và đăng nhập lại."
      });
      setShowOtpModalForgotPassword(false);

      // Đăng xuất sau khi reset mật khẩu
      setTimeout(() => {
        authService.logout();
        window.location.href = "/login";
      }, 3000);

    } catch (err) {
      setToast({ type: "error", message: err.response?.data || "Reset mật khẩu thất bại!" });
    } finally {
      setLoading(false);
    }
  };

  // Vô hiệu hóa tài khoản - Gửi OTP trước
  const handleDeactivateAccount = async () => {
    if (!deactivateReason.trim()) {
      setToast({ type: "error", message: "Vui lòng nhập lý do vô hiệu hóa tài khoản" });
      return;
    }

    setLoading(true);
    try {
      await instance.post("Auth/SendOTP", { email: profileData.email });
      setToast({ type: "success", message: "Mã OTP đã gửi đến email!" });
      setShowDeactivateModal(false);
      setShowOtpModalDeactivate(true);
    } catch (err) {
      setToast({ type: "error", message: err.response?.data || "Gửi OTP thất bại!" });
    } finally {
      setLoading(false);
    }
  };

  // Xác nhận vô hiệu hóa sau OTP
  const handleDeactivateAfterOtp = async () => {
    setLoading(true);
    try {
      await instance.put("Auth/DeactivateAccount");
      setToast({
        type: "warning",
        message: "Tài khoản đã được vô hiệu hóa. Bạn sẽ đăng xuất."
      });
      setShowOtpModalDeactivate(false);

      // Đăng xuất sau khi vô hiệu hóa
      setTimeout(() => {
        authService.logout();
        window.location.href = "/login";
      }, 3000);

    } catch (err) {
      setToast({ type: "error", message: err.response?.data || "Vô hiệu hóa tài khoản thất bại!" });
    } finally {
      setLoading(false);
    }
  };

  // ✅ Get Location
  const handleGetLocation = () => {
    if (!navigator.geolocation) {
      setToast({ type: "error", message: "Trình duyệt không hỗ trợ định vị." });
      return;
    }

    setToast({ type: "info", message: "Đang xác định vị trí..." });

    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const { latitude, longitude } = position.coords;

        try {
          const response = await fetch(
            `https://nominatim.openstreetmap.org/reverse?lat=${latitude}&lon=${longitude}&zoom=18&addressdetails=1&format=json&accept-language=vi`
          );
          const data = await response.json();

          if (data?.display_name) {
            setProfileData((prev) => ({ ...prev, address: data.display_name }));
            setButtonSave(true);
            setToast({ type: "success", message: "Đã lấy vị trí thành công" });
          } else {
            setToast({ type: "error", message: "Không tìm được địa chỉ từ vị trí GPS." });
          }
        } catch (error) {
          setToast({ type: "error", message: "Lỗi khi lấy địa chỉ từ GPS." });
        }
      },
      () => {
        setToast({ type: "error", message: "Không thể truy cập GPS. Hãy bật định vị." });
      },
      {
        enableHighAccuracy: true,
        timeout: 15000,
        maximumAge: 0
      }
    );
  };

  // Xử lý đăng xuất
  const handleLogout = () => {
    authService.logout();
    window.location.href = "/login";
  };

  return (
    <>
      <Loading isLoading={loading} />

      <div className="container-fluid py-4 bg-light min-vh-100">
        {/* Toast */}
        {toast && (
          <CustomToast
            type={toast.type}
            message={toast.message}
            onClose={() => setToast(null)}
          />
        )}

        <div className="row justify-content-center">
          <div className="col-xxl-11 col-xl-10 col-lg-10 col-md-12">
            {/* HỒ SƠ BỆNH NHÂN */}
            <Card className="mb-4 shadow-sm border-0">
              <Card.Header className="bg-white border-bottom py-3">
                <h4 className="mb-0 fw-bold text-dark">
                  <i className="bi bi-person-badge me-2 text-primary"></i>
                  Hồ sơ bệnh nhân
                </h4>
              </Card.Header>
              <Card.Body className="p-4">
                <h5 className="mb-4 fw-semibold text-dark border-bottom pb-2">
                  <i className="bi bi-info-circle me-2 text-secondary"></i>
                  Thông tin cá nhân
                </h5>
                <Form onSubmit={handleConfirmUpdateProfile}>
                  <Row className="g-3">
                    <Col md={6}>
                      <Form.Group>
                        <Form.Label className="fw-semibold text-dark">
                          <i className="bi bi-person me-1 text-muted"></i>
                          Tên tài khoản
                        </Form.Label>
                        <Form.Control
                          type="text"
                          value={profileData.username}
                          disabled
                          className="bg-light"
                        />
                        <Form.Text className="text-muted">
                          Tên tài khoản không thể thay đổi
                        </Form.Text>
                      </Form.Group>
                    </Col>
                    <Col md={6}>
                      <Form.Group>
                        <Form.Label className="fw-semibold text-dark">
                          <i className="bi bi-envelope me-1 text-muted"></i>
                          Email
                        </Form.Label>
                        <Form.Control
                          type="email"
                          value={profileData.email}
                          disabled
                          className="bg-light"
                        />
                        <Form.Text className="text-muted">
                          Email không thể thay đổi
                        </Form.Text>
                      </Form.Group>
                    </Col>
                    <Col md={6}>
                      <Form.Group>
                        <Form.Label className="fw-semibold text-dark">
                          <i className="bi bi-card-heading me-1 text-muted"></i>
                          Họ và tên <span className="text-danger">*</span>
                        </Form.Label>
                        <Form.Control
                          type="text"
                          name="fullName"
                          value={profileData.fullName}
                          onChange={handleProfileChange}
                          isInvalid={!!errors.fullName}
                          className="border-secondary-subtle"
                        />
                        <Form.Control.Feedback type="invalid">
                          {errors.fullName}
                        </Form.Control.Feedback>
                      </Form.Group>
                    </Col>
                    <Col md={6}>
                      <Form.Group>
                        <Form.Label className="fw-semibold text-dark">
                          <i className="bi bi-telephone me-1 text-muted"></i>
                          Số điện thoại <span className="text-danger">*</span>
                        </Form.Label>
                        <Form.Control
                          type="text"
                          name="phone"
                          value={profileData.phone}
                          onChange={handleProfileChange}
                          isInvalid={!!errors.phone}
                          className="border-secondary-subtle"
                        />
                        <Form.Control.Feedback type="invalid">
                          {errors.phone}
                        </Form.Control.Feedback>
                      </Form.Group>
                    </Col>
                    <Col md={3}>
                      <Form.Group>
                        <Form.Label className="fw-semibold text-dark">
                          <i className="bi bi-calendar me-1 text-muted"></i>
                          Ngày sinh <span className="text-danger">*</span>
                        </Form.Label>
                        <Form.Control
                          type="date"
                          name="dateOfBirth"
                          value={profileData.dateOfBirth}
                          onChange={handleProfileChange}
                          isInvalid={!!errors.dateOfBirth}
                          className="border-secondary-subtle"
                        />
                        <Form.Control.Feedback type="invalid">
                          {errors.dateOfBirth}
                        </Form.Control.Feedback>
                      </Form.Group>
                    </Col>
                    <Col md={3}>
                      <Form.Group>
                        <Form.Label className="fw-semibold text-dark">
                          <i className="bi bi-gender-ambiguous me-1 text-muted"></i>
                          Giới tính <span className="text-danger">*</span>
                        </Form.Label>
                        <Form.Select
                          name="gender"
                          value={profileData.gender}
                          onChange={handleProfileChange}
                          isInvalid={!!errors.gender}
                          className="border-secondary-subtle"
                        >
                          <option value="">-- Chọn giới tính --</option>
                          <option value="Nam">Nam</option>
                          <option value="Nữ">Nữ</option>
                        </Form.Select>
                        <Form.Control.Feedback type="invalid">
                          {errors.gender}
                        </Form.Control.Feedback>
                      </Form.Group>
                    </Col>
                    <Col md={6}>
                      <Form.Group>
                        <Form.Label className="fw-semibold text-dark">
                          <i className="bi bi-geo-alt me-1 text-muted"></i>
                          Địa chỉ <span className="text-danger">*</span>
                        </Form.Label>
                        <InputGroup>
                          <Form.Control
                            type="text"
                            name="address"
                            value={profileData.address}
                            onChange={handleProfileChange}
                            isInvalid={!!errors.address}
                            className="border-secondary-subtle"
                            placeholder="Nhập địa chỉ hoặc sử dụng nút bên cạnh để lấy vị trí"
                          />
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
                    </Col>
                  </Row>
                  <div className="mt-4 pt-3 border-top">
                    <Button
                      type="submit"
                      variant="primary"
                      disabled={loading || !buttonSave}
                      className="px-4 me-3"
                    >
                      <i className="bi bi-check-circle me-2"></i>
                      {loading ? "Đang xử lý..." : "Lưu thay đổi"}
                    </Button>
                    {!buttonSave && (
                      <Form.Text className="text-muted">
                        Chưa có thay đổi nào để lưu
                      </Form.Text>
                    )}
                  </div>
                </Form>
              </Card.Body>
            </Card>

            {/* ĐỔI MẬT KHẨU */}
            <Card className="mb-4 shadow-sm border-0">
              <Card.Header className="bg-white border-bottom py-3">
                <h4 className="mb-0 fw-bold text-dark">
                  <i className="bi bi-shield-lock me-2 text-primary"></i>
                  Bảo mật tài khoản
                </h4>
              </Card.Header>
              <Card.Body className="p-4">
                <h6 className="mb-3 fw-semibold text-dark">
                  <i className="bi bi-key me-2 text-secondary"></i>
                  Đổi mật khẩu
                </h6>
                <Form onSubmit={handleConfirmChangePassword}>
                  <Row className="g-3">
                    <Col md={12}>
                      <Form.Group>
                        <Form.Label className="fw-semibold text-dark">
                          Mật khẩu hiện tại
                        </Form.Label>
                        <InputGroup>
                          <Form.Control
                            type={showPassword ? "text" : "password"}
                            name="currentPassword"
                            value={passwordData.currentPassword}
                            onChange={handlePasswordChange}
                            isInvalid={!!errors.currentPassword}
                            disabled={loading}
                            className="border-secondary-subtle"
                            placeholder="Nhập mật khẩu hiện tại"
                          />
                          <Button
                            variant="outline-secondary"
                            onClick={() => setShowPassword(!showPassword)}
                          >
                            <i className={showPassword ? "bi bi-eye-slash" : "bi bi-eye"}></i>
                          </Button>
                          <Form.Control.Feedback type="invalid">
                            {errors.currentPassword}
                          </Form.Control.Feedback>
                        </InputGroup>
                      </Form.Group>
                    </Col>
                    <Col md={6}>
                      <Form.Group>
                        <Form.Label className="fw-semibold text-dark">
                          Mật khẩu mới
                        </Form.Label>
                        <Form.Control
                          type={showPassword ? "text" : "password"}
                          name="newPassword"
                          value={passwordData.newPassword}
                          onChange={handlePasswordChange}
                          isInvalid={!!errors.newPassword}
                          disabled={loading}
                          className="border-secondary-subtle"
                          placeholder="Nhập mật khẩu mới"
                        />
                        <Form.Control.Feedback type="invalid">
                          {errors.newPassword}
                        </Form.Control.Feedback>
                        <Form.Text className="text-muted">
                          Mật khẩu ít nhất 6 ký tự
                        </Form.Text>
                      </Form.Group>
                    </Col>
                    <Col md={6}>
                      <Form.Group>
                        <Form.Label className="fw-semibold text-dark">
                          Xác nhận mật khẩu mới
                        </Form.Label>
                        <Form.Control
                          type={showPassword ? "text" : "password"}
                          name="confirmPassword"
                          value={passwordData.confirmPassword}
                          onChange={handlePasswordChange}
                          isInvalid={!!errors.confirmPassword}
                          disabled={loading}
                          className="border-secondary-subtle"
                          placeholder="Xác nhận mật khẩu mới"
                        />
                        <Form.Control.Feedback type="invalid">
                          {errors.confirmPassword}
                        </Form.Control.Feedback>
                      </Form.Group>
                    </Col>
                  </Row>
                  <div className="mt-3">
                    <Button
                      type="submit"
                      variant="outline-primary"
                      disabled={loading}
                      className="px-4 me-2"
                    >
                      <i className="bi bi-shield-check me-2"></i>
                      {loading ? "Đang kiểm tra..." : "Đổi mật khẩu"}
                    </Button>

                    <Button
                      variant="outline-secondary"
                      onClick={() => setShowForgotPasswordModal(true)}
                      className="px-3"
                    >
                      <i className="bi bi-question-circle me-2"></i>
                      Quên mật khẩu?
                    </Button>
                  </div>
                </Form>

                {/* Các chức năng bảo mật khác */}
                <div className="mt-4 pt-3 border-top">
                  <h6 className="mb-3 fw-semibold text-dark">
                    <i className="bi bi-gear me-2 text-secondary"></i>
                    Tùy chọn tài khoản
                  </h6>
                  <div className="d-flex gap-2 flex-wrap">
                    <Button
                      variant="outline-warning"
                      onClick={() => setShowDeactivateModal(true)}
                      className="px-3"
                    >
                      <i className="bi bi-person-x me-2"></i>
                      Vô hiệu hóa tài khoản
                    </Button>
                  </div>
                </div>
              </Card.Body>
            </Card>
          </div>
        </div>
      </div>

      {/* OTP Modal cho các chức năng */}
      <OtpModal
        show={showOtpModalPassword}
        email={profileData.email}
        onClose={() => setShowOtpModalPassword(false)}
        onVerified={handleChangePasswordAfterOtp}
      />
      <OtpModal
        show={showOtpModalUpdate}
        email={profileData.email}
        onClose={() => setShowOtpModalUpdate(false)}
        onVerified={handleSaveProfile}
      />
      <OtpModal
        show={showOtpModalDeactivate}
        email={profileData.email}
        onClose={() => setShowOtpModalDeactivate(false)}
        onVerified={handleDeactivateAfterOtp}
      />
      <OtpModal
        show={showOtpModalForgotPassword}
        email={profileData.email}
        onClose={() => setShowOtpModalForgotPassword(false)}
        onVerified={handleForgotPasswordAfterOtp}
      />

      {/* Modal Quên mật khẩu */}
      <Modal show={showForgotPasswordModal} onHide={() => setShowForgotPasswordModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title>
            <i className="bi bi-question-circle me-2 text-warning"></i>
            Quên mật khẩu
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p>Bạn sẽ nhận được OTP xác nhận và sau đó hệ thống sẽ gửi mật khẩu mới đến email của bạn. Bạn có chắc chắn muốn tiếp tục?</p>
          <div className="alert alert-info">
            <i className="bi bi-info-circle me-2"></i>
            Mã OTP và mật khẩu mới sẽ được gửi đến: <strong>{profileData.email}</strong>
          </div>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowForgotPasswordModal(false)}>
            Hủy
          </Button>
          <Button variant="warning" onClick={handleForgotPassword}>
            <i className="bi bi-send me-2"></i>
            Gửi yêu cầu
          </Button>
        </Modal.Footer>
      </Modal>

      {/* Modal Vô hiệu hóa tài khoản */}
      <Modal show={showDeactivateModal} onHide={() => setShowDeactivateModal(false)} centered>
        <Modal.Header closeButton className="bg-warning text-dark">
          <Modal.Title>
            <i className="bi bi-exclamation-triangle me-2"></i>
            Vô hiệu hóa tài khoản
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <div className="alert alert-warning">
            <i className="bi bi-exclamation-circle me-2"></i>
            <strong>Cảnh báo:</strong> Tài khoản của bạn sẽ tạm thời bị vô hiệu hóa. Bạn không thể đăng nhập cho đến khi kích hoạt lại.
          </div>

          <Form.Group>
            <Form.Label className="fw-semibold">
              Lý do vô hiệu hóa <span className="text-danger">*</span>
            </Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              value={deactivateReason}
              onChange={(e) => setDeactivateReason(e.target.value)}
              placeholder="Vui lòng cho chúng tôi biết lý do bạn muốn vô hiệu hóa tài khoản..."
              className="border-warning"
            />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowDeactivateModal(false)}>
            Hủy bỏ
          </Button>
          <Button variant="warning" onClick={handleDeactivateAccount}>
            <i className="bi bi-person-x me-2"></i>
            Vô hiệu hóa tài khoản
          </Button>
        </Modal.Footer>
      </Modal>

      {/* Modal bắt buộc đổi mật khẩu */}
      <Modal show={showForceChangePasswordModal} onHide={() => { }} centered backdrop="static" keyboard={false}>
        <Modal.Header className="bg-danger text-white">
          <Modal.Title>
            <i className="bi bi-exclamation-triangle-fill me-2"></i>
            Bắt buộc đổi mật khẩu
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <div className="alert alert-danger">
            <i className="bi bi-shield-exclamation me-2"></i>
            <strong>Bảo mật quan trọng:</strong> Bạn cần phải đổi mật khẩu để tiếp tục sử dụng hệ thống.
          </div>
          <p className="mb-3">Vui lòng đổi mật khẩu ngay bây giờ để đảm bảo an toàn cho tài khoản của bạn.</p>

          <div className="d-grid gap-2">
            <Button
              variant="danger"
              onClick={() => {
                setShowForceChangePasswordModal(false);
                // Focus vào phần đổi mật khẩu
                document.getElementById('security-section')?.scrollIntoView({ behavior: 'smooth' });
              }}
            >
              <i className="bi bi-key me-2"></i>
              Đổi mật khẩu ngay
            </Button>
          </div>
        </Modal.Body>
      </Modal>
    </>
  );
};

export default PatientProfile;