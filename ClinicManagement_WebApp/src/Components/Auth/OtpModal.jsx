// OtpModal.jsx - PHIÊN BẢN HOÀN HẢO: ĐẸP + CHỐNG SPAM + ĐẾM NGƯỢC
import React, { useState, useEffect } from "react";
import { Modal } from "react-bootstrap";
import instance from "../../axios";

const OtpModal = ({ show, email, onClose, onVerified }) => {
    const [otp, setOtp] = useState(["", "", "", "", "", ""]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    // Tính năng GỬI LẠI MÃ + ĐẾM NGƯỢC
    const [resendCooldown, setResendCooldown] = useState(0);
    const [isResending, setIsResending] = useState(false);

    // Đếm ngược 60s
    useEffect(() => {
        if (resendCooldown > 0) {
            const timer = setTimeout(() => {
                setResendCooldown(resendCooldown - 1);
            }, 1000);
            return () => clearTimeout(timer);
        }
    }, [resendCooldown]);

    // Gửi lại mã OTP
    const handleResendOTP = async () => {
        if (resendCooldown > 0 || isResending) return;

        setIsResending(true);
        try {
            const res = await instance.post("Auth/SendOTP", { email });
            setError("");
            setResendCooldown(120);
            setOtp(["", "", "", "", "", ""]);
            setToast({ type: "success", message: "Đã gửi lại mã OTP!" });
        } catch (err) {
            const msg = err.response?.data || "Gửi lại thất bại!";
            setError(msg);
        } finally {
            setIsResending(false);
        }
    };

    // Xử lý nhập OTP
    const handleChange = (value, index) => {
        if (!/^\d*$/.test(value)) return;
        const newOtp = [...otp];
        newOtp[index] = value.slice(-1);
        setOtp(newOtp);
        if (value && index < 5) {
            document.getElementById(`otp-${index + 1}`)?.focus();
        }
    };

    const handleKeyDown = (e, index) => {
        if (e.key === "Backspace" && !otp[index] && index > 0) {
            document.getElementById(`otp-${index - 1}`)?.focus();
        }
    };

    // Xác nhận OTP
    const handleVerify = async () => {
        const code = otp.join("");
        if (code.length !== 6) {
            setError("Vui lòng nhập đủ 6 số");
            return;
        }

        setLoading(true);
        try {
            await instance.post("Auth/VerifyOTP", { email, otp: code });
            onVerified();
            onClose();
            //setToast
        } catch (err) {
            setError("Mã OTP không đúng hoặc đã hết hạn!");
            setOtp(["", "", "", "", "", ""]);
        } finally {
            setLoading(false);
        }
    };

    // Reset khi mở modal
    useEffect(() => {
        if (show) {
            setOtp(["", "", "", "", "", ""]);
            setError("");
            setResendCooldown(120); // Mở modal là bắt đầu đếm ngược luôn
        }
    }, [show]);

    return (
        <Modal show={show} onHide={onClose} centered backdrop="static" keyboard={false}>
            <Modal.Body className="p-4">
                <div className="text-center mb-4">
                    <h5 className="fw-bold fs-4">Nhập mã xác nhận</h5>
                    <p className="text-muted small lh-lg">
                        Vui lòng nhập mã xác nhận để đăng ký tài khoản truy cập hệ thống.
                        Mã xác nhận đã được gửi tới số điện thoại <br />
                        <strong className="text-dark">••••••{email?.slice(-4)}</strong>
                    </p>
                </div>

                {/* 6 ô OTP */}
                <div className="d-flex justify-content-center gap-3 mb-4">
                    {otp.map((digit, i) => (
                        <input
                            key={i}
                            id={`otp-${i}`}
                            type="text"
                            maxLength="1"
                            value={digit}
                            onChange={(e) => handleChange(e.target.value, i)}
                            onKeyDown={(e) => handleKeyDown(e, i)}
                            className="form-control text-center fw-bold"
                            style={{
                                width: "54px",
                                height: "60px",
                                fontSize: "1.8rem",
                                borderRadius: "12px",
                                border: "2px solid #e0e0e0",
                                backgroundColor: digit ? "#fff" : "#f9f9f9",
                            }}
                            disabled={loading}
                        />
                    ))}
                </div>

                {/* Lỗi */}
                {error && <div className="text-danger text-center small mb-3">{error}</div>}

                {/* Thông báo không nhận được mã */}
                <div className="text-center text-danger small mb-2">
                    Tôi không nhận được mã OTP vui lòng gửi lại
                </div>

                {/* Nút Gửi lại mã + đếm ngược */}
                <div className="text-center mb-4">
                    {resendCooldown > 0 ? (
                        <span className="text-muted small">
                            Gửi lại mã sau <strong>{resendCooldown}s</strong>
                        </span>
                    ) : (
                        <button
                            onClick={handleResendOTP}
                            disabled={isResending}
                            className="btn btn-link text-primary p-0 text-decoration-none small"
                            style={{ fontWeight: "500" }}
                        >
                            {isResending ? "Đang gửi..." : "Gửi lại mã"}
                        </button>
                    )}
                </div>

                {/* Checkbox */}
                <div className="d-flex justify-content-center align-items-center gap-2 mb-4">
                    <input className="form-check-input" type="checkbox" defaultChecked style={{ width: "18px", height: "18px" }} />
                    <label className="form-check-label text-muted small">
                        Tôi xin chấp nhận điều khoản cam kết
                    </label>
                </div>

                {/* Nút Tiếp tục */}
                <button
                    className="btn w-100 text-white fw-bold"
                    style={{
                        backgroundColor: "#ee2b3a",
                        border: "none",
                        height: "56px",
                        fontSize: "1.1rem",
                        borderRadius: "12px",
                    }}
                    onClick={handleVerify}
                    disabled={loading || otp.join("").length !== 6}
                >
                    {loading ? "Đang xác thực..." : "Tiếp tục"}
                </button>
            </Modal.Body>
        </Modal>
    );
};

export default OtpModal;