// PatientBooking.jsx - TRANG ĐẶT LỊCH KHÁM (ĐÃ TÍCH HỢP API VÀ CONFIRM DELETE MODAL)
import React, { useState, useEffect } from "react";
import { Card, Form, Row, Col, Button, Table, Modal, Badge } from "react-bootstrap";
import Loading from "../../Components/Loading/Loading";
import CustomToast from "../../Components/CustomToast/CustomToast";
import instance from "../../axios";
import ConfirmDeleteModal from "../../Components/CustomToast/DeleteConfirmModal";

const PatientBooking = () => {
    const [loading, setLoading] = useState(false);
    const [toast, setToast] = useState(null);
    const [showConfirmModal, setShowConfirmModal] = useState(false);
    const [showCancelModal, setShowCancelModal] = useState(false);
    const [selectedAppointment, setSelectedAppointment] = useState(null);

    // Dữ liệu form đặt lịch
    const [appointmentData, setAppointmentData] = useState({
        appointmentDate: "",
        appointmentTime: "",
        notes: ""
    });

    // Dữ liệu từ API
    const [availableSlots, setAvailableSlots] = useState([]);
    const [appointments, setAppointments] = useState([]);

    // Load danh sách lịch hẹn khi component mount
    useEffect(() => {
        fetchMyAppointments();
    }, []);

    // API: Lấy danh sách lịch hẹn của tôi
    const fetchMyAppointments = async () => {
        setLoading(true);
        try {
            const response = await instance.get("User/GetMyAppointment");
            console.log(response);

            if (response.status === "Success" && response.content) {
                setAppointments([...response.content]);
            } else {
                setToast({ type: "error", message: response.message || "Không thể tải danh sách lịch hẹn" });
            }
        } catch (error) {
            console.error("Error fetching appointments:", error);
            setToast({ type: "error", message: "Lỗi khi tải danh sách lịch hẹn" });
        } finally {
            setLoading(false);
        }
    };

    // API: Lấy khung giờ khả dụng
    const fetchAvailableTimeSlots = async (date) => {
        setLoading(true);
        try {
            const response = await instance.get(`User/GetAvailableTimeSlots/${date}`);
            if (response.status === "Success" && response.content) {
                setAvailableSlots(response.content);
            } else {
                setToast({ type: "error", message: response.message || "Không thể tải khung giờ khả dụng" });
                setAvailableSlots([]);
            }
        } catch (error) {
            console.error("Error fetching time slots:", error);
            setToast({ type: "error", message: "Lỗi khi tải khung giờ khả dụng" });
            setAvailableSlots([]);
        } finally {
            setLoading(false);
        }
    };

    // API: Tạo lịch hẹn mới
    const createAppointment = async () => {
        setLoading(true);
        try {
            const payload = {
                appointmentDate: appointmentData.appointmentDate,
                appointmentTime: appointmentData.appointmentTime,
                notes: appointmentData.notes || ""
            };

            const response = await instance.post("User/CreateAppointmentByPatient", payload);

            if (response.status === "Success") {
                setToast({ type: "success", message: response.message || "Đặt lịch khám thành công!" });

                // Reset form
                setAppointmentData({
                    appointmentDate: "",
                    appointmentTime: "",
                    notes: ""
                });
                setAvailableSlots([]);
                setShowConfirmModal(false);

                // Reload danh sách lịch hẹn
                await fetchMyAppointments();
            } else {
                setToast({ type: "error", message: response.message || "Đặt lịch thất bại!" });
            }
        } catch (error) {
            console.error("Error creating appointment:", error);
            const errorMessage = error.response?.data?.message || "Lỗi khi đặt lịch khám";
            setToast({ type: "error", message: errorMessage });
        } finally {
            setLoading(false);
        }
    };

    // Xử lý thay đổi form
    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setAppointmentData(prev => ({
            ...prev,
            [name]: value
        }));

        // Nếu chọn ngày thì gọi API lấy khung giờ khả dụng
        if (name === "appointmentDate" && value) {
            fetchAvailableTimeSlots(value);
        }

        // Nếu xóa ngày thì clear khung giờ
        if (name === "appointmentDate" && !value) {
            setAvailableSlots([]);
            setAppointmentData(prev => ({ ...prev, appointmentTime: "" }));
        }
    };

    // Validate form
    const validateForm = () => {
        if (!appointmentData.appointmentDate) {
            setToast({ type: "error", message: "Vui lòng chọn ngày khám" });
            return false;
        }
        if (!appointmentData.appointmentTime) {
            setToast({ type: "error", message: "Vui lòng chọn giờ khám" });
            return false;
        }

        // Kiểm tra ngày không được trong quá khứ
        const selectedDate = new Date(appointmentData.appointmentDate);
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        if (selectedDate < today) {
            setToast({ type: "error", message: "Không thể đặt lịch trong quá khứ" });
            return false;
        }

        return true;
    };

    // Xử lý đặt lịch
    const handleBookAppointment = (e) => {
        e.preventDefault();
        if (!validateForm()) return;
        setShowConfirmModal(true);
    };

    // Xác nhận đặt lịch
    const confirmBooking = () => {
        createAppointment();
    };

    // Mở modal xác nhận hủy lịch hẹn
    const handleShowCancelModal = (appointment) => {
        setSelectedAppointment(appointment);
        setShowCancelModal(true);
    };

    // Đóng modal hủy lịch hẹn
    const handleCloseCancelModal = () => {
        setShowCancelModal(false);
        setSelectedAppointment(null);
    };

    // Xác nhận hủy lịch hẹn
    const handleConfirmCancel = async () => {
        if (!selectedAppointment) return;

        setLoading(true);
        try {
            const response = await instance.put(`Appointment/AppointmentCancelAsync/${selectedAppointment.appointmentId}`);

            if (response.status === "Success") {
                setToast({ type: "success", message: response.message || "Hủy lịch hẹn thành công!" });
                fetchMyAppointments();
            } else {
                setToast({ type: "error", message: response.message || "Hủy lịch hẹn thất bại!" });
            }
        } catch (error) {
            console.error("Error canceling appointment:", error);
            setToast({ type: "error", message: error?.response?.data?.message || "Lỗi khi hủy lịch hẹn" });
        } finally {
            setLoading(false);
            setShowCancelModal(false);
            setSelectedAppointment(null);
        }
    };

    // Format ngày tháng
    const formatDate = (dateString) => {
        if (!dateString) return "";
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    };

    // Format time từ TimeOnly
    const formatTime = (timeString) => {
        if (!timeString) return "";
        // Nếu timeString đã ở dạng HH:mm thì return luôn
        if (typeof timeString === 'string' && timeString.includes(':')) {
            return timeString;
        }
        // Nếu là TimeOnly object từ API
        try {
            const time = new Date(`1970-01-01T${timeString}`);
            return time.toLocaleTimeString('vi-VN', {
                hour: '2-digit',
                minute: '2-digit',
                hour12: false
            });
        } catch {
            return timeString;
        }
    };

    // Lấy badge color theo status
    const getStatusBadge = (status) => {
        switch (status) {
            case 'Ordered':
            case 'Đã đặt':
                return 'primary';
            case 'Pending':
            case 'Đang chờ':
                return 'warning';
            case 'Completed':
            case 'Đã khám':
                return 'success';
            case 'Canceled':
            case 'Hủy':
                return 'danger';
            default:
                return 'secondary';
        }
    };

    // Map status từ tiếng Anh sang tiếng Việt
    const getStatusText = (status) => {
        const statusMap = {
            'Ordered': 'Đã đặt',
            'Pending': 'Đang chờ',
            'Completed': 'Đã khám',
            'Canceled': 'Đã hủy'
        };
        return statusMap[status] || status;
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
                    <div className="col-xxl-10 col-xl-12 col-lg-12 col-md-12">
                        <div className="row">
                            {/* Form đặt lịch */}
                            <div className="col-lg-6 mb-4">
                                <Card className="shadow-sm border-0 h-100">
                                    <Card.Header className="bg-white border-bottom py-3">
                                        <h4 className="mb-0 fw-bold text-dark">
                                            <i className="bi bi-calendar-plus me-2 text-primary"></i>
                                            Đặt lịch khám mới
                                        </h4>
                                    </Card.Header>
                                    <Card.Body className="p-4">
                                        <Form onSubmit={handleBookAppointment}>
                                            {/* Chọn ngày */}
                                            <Form.Group className="mb-4">
                                                <Form.Label className="fw-semibold text-dark">
                                                    <i className="bi bi-calendar-date me-1 text-muted"></i>
                                                    Chọn ngày khám <span className="text-danger">*</span>
                                                </Form.Label>
                                                <Form.Control
                                                    type="date"
                                                    name="appointmentDate"
                                                    value={appointmentData.appointmentDate}
                                                    onChange={handleInputChange}
                                                    min={new Date().toISOString().split('T')[0]}
                                                    className="border-secondary-subtle"
                                                    required
                                                    disabled={loading}
                                                />
                                                <Form.Text className="text-muted">
                                                    Chọn ngày bạn muốn đến khám
                                                </Form.Text>
                                            </Form.Group>

                                            {/* Chọn giờ */}
                                            {appointmentData.appointmentDate && (
                                                <Form.Group className="mb-4">
                                                    <Form.Label className="fw-semibold text-dark">
                                                        <i className="bi bi-clock me-1 text-muted"></i>
                                                        Chọn giờ khám <span className="text-danger">*</span>
                                                    </Form.Label>
                                                    {availableSlots.length === 0 ? (
                                                        <div className="text-center py-3">
                                                            <span className="text-muted">Không có lịch hẹn trong khoản thời gian này</span>
                                                        </div>
                                                    ) : (
                                                        <div className="row g-2">
                                                            {availableSlots.map((slot, index) => (
                                                                <div key={index} className="col-4">
                                                                    <Button
                                                                        variant={
                                                                            appointmentData.appointmentTime === slot.time
                                                                                ? "primary"
                                                                                : slot.available
                                                                                    ? "outline-primary"
                                                                                    : "outline-secondary"
                                                                        }
                                                                        className="w-100 mb-2 position-relative"
                                                                        onClick={() => slot.available && setAppointmentData(prev => ({
                                                                            ...prev,
                                                                            appointmentTime: slot.time
                                                                        }))}
                                                                        disabled={!slot.available || loading}
                                                                    >
                                                                        {slot.time}
                                                                        {!slot.available && (
                                                                            <small className="d-block text-muted">Đã kín</small>
                                                                        )}
                                                                        {slot.available && slot.bookedCount > 0 && (
                                                                            <small className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-warning">
                                                                                {slot.bookedCount}
                                                                            </small>
                                                                        )}
                                                                    </Button>
                                                                </div>
                                                            ))}
                                                        </div>
                                                    )}
                                                    {availableSlots.length > 0 && availableSlots.every(slot => !slot.available) && (
                                                        <div className="alert alert-warning mt-2 mb-0">
                                                            <small>
                                                                <i className="bi bi-exclamation-triangle me-1"></i>
                                                                Tất cả khung giờ đã kín. Vui lòng chọn ngày khác.
                                                            </small>
                                                        </div>
                                                    )}
                                                </Form.Group>
                                            )}

                                            {/* Ghi chú */}
                                            <Form.Group className="mb-4">
                                                <Form.Label className="fw-semibold text-dark">
                                                    <i className="bi bi-chat-text me-1 text-muted"></i>
                                                    Ghi chú (tùy chọn)
                                                </Form.Label>
                                                <Form.Control
                                                    as="textarea"
                                                    rows={3}
                                                    name="notes"
                                                    value={appointmentData.notes}
                                                    onChange={handleInputChange}
                                                    className="border-secondary-subtle"
                                                    placeholder="Mô tả triệu chứng hoặc yêu cầu đặc biệt..."
                                                    disabled={loading}
                                                />
                                            </Form.Group>

                                            <div className="d-grid">
                                                <Button
                                                    type="submit"
                                                    variant="primary"
                                                    size="lg"
                                                    disabled={!appointmentData.appointmentDate || !appointmentData.appointmentTime || loading}
                                                    className="fw-semibold"
                                                >
                                                    {loading ? (
                                                        <>
                                                            <div className="spinner-border spinner-border-sm me-2"></div>
                                                            Đang xử lý...
                                                        </>
                                                    ) : (
                                                        <>
                                                            <i className="bi bi-check-circle me-2"></i>
                                                            Đặt lịch khám
                                                        </>
                                                    )}
                                                </Button>
                                            </div>
                                        </Form>
                                    </Card.Body>
                                </Card>
                            </div>

                            {/* Lịch sử đặt lịch */}
                            <div className="col-lg-6">
                                <Card className="shadow-sm border-0 h-100">
                                    <Card.Header className="bg-white border-bottom py-3">
                                        <h4 className="mb-0 fw-bold text-dark">
                                            <i className="bi bi-clock-history me-2 text-primary"></i>
                                            Lịch hẹn của tôi
                                        </h4>
                                    </Card.Header>
                                    <Card.Body className="p-0 " style={{ height: 'calc(100vh - 200px)' }}>
                                        {loading ? (
                                            <div className="text-center py-5">
                                                <div className="spinner-border text-primary"></div>
                                                <p className="text-muted mt-3">Đang tải lịch hẹn...</p>
                                            </div>
                                        ) : appointments.length === 0 ? (
                                            <div className="text-center py-5">
                                                <i className="bi bi-calendar-x display-4 text-muted"></i>
                                                <p className="text-muted mt-3">Chưa có lịch hẹn nào</p>
                                            </div>
                                        ) : (
                                            <div className="table-responsive">
                                                <Table hover className="mb-0">
                                                    <thead className="bg-light">
                                                        <tr>
                                                            <th className="border-0">Ngày giờ</th>
                                                            <th className="border-0">Bác sĩ</th>
                                                            <th className="border-0">Trạng thái</th>
                                                            <th className="border-0">Thao tác</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        {appointments.map((appointment, index) => (
                                                            <tr key={index}>
                                                                <td>
                                                                    <div className="fw-semibold">
                                                                        {formatDate(appointment.appointmentDate)}
                                                                    </div>
                                                                    <small className="text-muted">
                                                                        {formatTime(appointment.appointmentTime)}
                                                                    </small>
                                                                    {appointment.notes && (
                                                                        <div>
                                                                            <small className="text-info">
                                                                                <i className="bi bi-chat-left-text me-1"></i>
                                                                                {appointment.notes}
                                                                            </small>
                                                                        </div>
                                                                    )}
                                                                </td>
                                                                <td>
                                                                    <div className="fw-semibold">
                                                                        {appointment.staffName || "Chưa phân công"}
                                                                    </div>
                                                                </td>
                                                                <td>
                                                                    <Badge bg={getStatusBadge(appointment.status)}>
                                                                        {getStatusText(appointment.status)}
                                                                    </Badge>
                                                                </td>
                                                                <td>
                                                                    {appointment.status === 'Ordered' && (
                                                                        <Button
                                                                            variant="outline-danger"
                                                                            size="sm"
                                                                            onClick={() => handleShowCancelModal(appointment)}
                                                                            disabled={loading}
                                                                        >
                                                                            <i className="bi bi-x-circle me-1"></i>
                                                                            Hủy
                                                                        </Button>
                                                                    )}
                                                                </td>
                                                            </tr>
                                                        ))}
                                                    </tbody>
                                                </Table>
                                            </div>
                                        )}
                                    </Card.Body>
                                </Card>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Modal xác nhận đặt lịch */}
            <Modal show={showConfirmModal} onHide={() => !loading && setShowConfirmModal(false)} centered>
                <Modal.Header closeButton>
                    <Modal.Title>
                        <i className="bi bi-calendar-check me-2 text-primary"></i>
                        Xác nhận đặt lịch
                    </Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div className="alert alert-info">
                        <i className="bi bi-info-circle me-2"></i>
                        Vui lòng kiểm tra kỹ thông tin trước khi xác nhận
                    </div>

                    <div className="bg-light p-3 rounded">
                        <Row>
                            <Col sm={4} className="fw-semibold">Ngày khám:</Col>
                            <Col sm={8}>{formatDate(appointmentData.appointmentDate)}</Col>
                        </Row>
                        <Row className="mt-2">
                            <Col sm={4} className="fw-semibold">Giờ khám:</Col>
                            <Col sm={8}>{appointmentData.appointmentTime}</Col>
                        </Row>
                        {appointmentData.notes && (
                            <Row className="mt-2">
                                <Col sm={4} className="fw-semibold">Ghi chú:</Col>
                                <Col sm={8}>{appointmentData.notes}</Col>
                            </Row>
                        )}
                    </div>

                    <div className="alert alert-warning mt-3">
                        <small>
                            <i className="bi bi-exclamation-triangle me-2"></i>
                            <strong>Lưu ý:</strong> Vui lòng đến trước 15 phút để làm thủ tục.
                            Hủy lịch trước 2 giờ nếu không thể đến.
                        </small>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <Button
                        variant="secondary"
                        onClick={() => setShowConfirmModal(false)}
                        disabled={loading}
                    >
                        Quay lại
                    </Button>
                    <Button
                        variant="primary"
                        onClick={confirmBooking}
                        disabled={loading}
                    >
                        {loading ? (
                            <>
                                <div className="spinner-border spinner-border-sm me-2"></div>
                                Đang xử lý...
                            </>
                        ) : (
                            <>
                                <i className="bi bi-check-circle me-2"></i>
                                Xác nhận đặt lịch
                            </>
                        )}
                    </Button>
                </Modal.Footer>
            </Modal>

            {/* Modal xác nhận hủy lịch hẹn */}
            <ConfirmDeleteModal
                isOpen={showCancelModal}
                title="Xác nhận hủy lịch hẹn"
                message={
                    selectedAppointment ? (
                        `Bạn có chắc chắn muốn hủy lịch hẹn ngày ${formatDate(selectedAppointment.appointmentDate)} lúc ${formatTime(selectedAppointment.appointmentTime)}?`
                    ) : "Bạn có chắc chắn muốn hủy lịch hẹn này?"
                }
                buttonLabel="Hủy lịch hẹn"
                onConfirm={handleConfirmCancel}
                onCancel={handleCloseCancelModal}
            />
        </>
    );
};

export default PatientBooking;