import React, { useState, useEffect, useCallback, useRef } from "react";
import { Card, Row, Col, Table, Badge, Button, Form, Modal, Accordion } from "react-bootstrap";
import instance from "../../axios";
import Loading from "../../Components/Loading/Loading";
import CustomToast from "../../Components/CustomToast/CustomToast";
import { format } from "date-fns";
import { useReactToPrint } from "react-to-print";

const PatientMedicalHistory = () => {
    const [medicalRecords, setMedicalRecords] = useState([]);
    const [selectedRecord, setSelectedRecord] = useState(null);
    const [selectedAppointment, setSelectedAppointment] = useState(null);
    const [loading, setLoading] = useState(false);
    const [toast, setToast] = useState(null);
    const [showDetailModal, setShowDetailModal] = useState(false);
    const [filters, setFilters] = useState({
        fromDate: "",
        toDate: "",
        status: ""
    });

    // Lấy danh sách lịch sử khám bệnh
    const fetchMedicalHistory = useCallback(async () => {
        setLoading(true);
        try {
            const response = await instance.get("User/GetMedicalRecordByPatient");
            if (response && response.content) {
                setMedicalRecords(response.content);
            } else {
                setMedicalRecords([]);
            }
        } catch (error) {
            console.error("Error fetching medical history:", error);
            setToast({ type: "error", message: "Không thể tải lịch sử khám bệnh" });
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchMedicalHistory();
    }, [fetchMedicalHistory]);

    // Mở modal chi tiết
    const openRecordDetail = (record, appointment) => {
        setSelectedRecord(record);
        setSelectedAppointment(appointment);
        setShowDetailModal(true);
    };

    // Lọc hồ sơ
    const filteredRecords = medicalRecords.filter(record => {
        if (!record.issuedDate) return false;

        const recordDate = new Date(record.issuedDate);
        const fromDate = filters.fromDate ? new Date(filters.fromDate) : null;
        const toDate = filters.toDate ? new Date(filters.toDate) : null;

        if (fromDate && recordDate < fromDate) return false;
        if (toDate && recordDate > toDate) return false;
        if (filters.status && record.recordStatus !== filters.status) return false;

        return true;
    });

    const handleFilterChange = (e) => {
        const { name, value } = e.target;
        setFilters(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const clearFilters = () => {
        setFilters({
            fromDate: "",
            toDate: "",
            status: ""
        });
    };

    // Format tiền tệ
    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount || 0);
    };

    // Format ngày tháng
    const formatDate = (dateString) => {
        if (!dateString) return 'N/A';
        return format(new Date(dateString), 'dd/MM/yyyy');
    };

    const formatTime = (timeString) => {
        if (!timeString) return '';
        return timeString.substring(0, 5);
    };

    return (
        <>
            <Loading isLoading={loading} />

            {toast && (
                <CustomToast
                    type={toast.type}
                    message={toast.message}
                    onClose={() => setToast(null)}
                />
            )}

            <div className="container-fluid py-3 bg-light min-vh-100">
                <div className="row justify-content-center">
                    <div className="col-12 col-lg-10 col-xl-11">
                        {/* Header */}
                        <div className="text-center mb-4">
                            <h4 className="fw-bold text-primary mb-2">
                                <i className="bi bi-clipboard2-pulse me-2"></i>
                                Lịch Sử Khám Bệnh
                            </h4>
                            <p className="text-muted">Theo dõi toàn bộ lịch sử khám bệnh và điều trị</p>
                        </div>

                        {/* Bộ lọc đơn giản */}
                        <Card className="mb-4 border-0 shadow-sm">
                            <Card.Body className="p-3">
                                <Row className="g-2 align-items-end">
                                    <Col md={4}>
                                        <Form.Group>
                                            <Form.Label className="small fw-medium text-dark">Từ ngày</Form.Label>
                                            <Form.Control
                                                type="date"
                                                name="fromDate"
                                                value={filters.fromDate}
                                                onChange={handleFilterChange}
                                                size="sm"
                                            />
                                        </Form.Group>
                                    </Col>
                                    <Col md={4}>
                                        <Form.Group>
                                            <Form.Label className="small fw-medium text-dark">Đến ngày</Form.Label>
                                            <Form.Control
                                                type="date"
                                                name="toDate"
                                                value={filters.toDate}
                                                onChange={handleFilterChange}
                                                size="sm"
                                            />
                                        </Form.Group>
                                    </Col>
                                    <Col md={3}>
                                        <Form.Group>
                                            <Form.Label className="small fw-medium text-dark">Trạng thái</Form.Label>
                                            <Form.Select
                                                name="status"
                                                value={filters.status}
                                                onChange={handleFilterChange}
                                                size="sm"
                                            >
                                                <option value="">Tất cả</option>
                                                <option value="Active">Đang hoạt động</option>
                                                <option value="Archived">Đã lưu trữ</option>
                                            </Form.Select>
                                        </Form.Group>
                                    </Col>
                                    <Col md={1}>
                                        <Button
                                            variant="outline-secondary"
                                            onClick={clearFilters}
                                            size="sm"
                                            className="w-100"
                                            title="Xóa bộ lọc"
                                        >
                                            <i className="bi bi-arrow-clockwise"></i>
                                        </Button>
                                    </Col>
                                </Row>
                            </Card.Body>
                        </Card>

                        {/* Danh sách hồ sơ */}
                        {filteredRecords.length === 0 ? (
                            <Card className="border-0 shadow-sm">
                                <Card.Body className="text-center py-5">
                                    <i className="bi bi-clipboard-x fs-1 text-muted d-block mb-3"></i>
                                    <h5 className="text-muted">{
                                        medicalRecords.length === 0 ?
                                            "Chưa có hồ sơ khám bệnh nào" :
                                            "Không tìm thấy hồ sơ phù hợp"
                                    }</h5>
                                </Card.Body>
                            </Card>
                        ) : (
                            filteredRecords.map((record) => (
                                <RecordCard
                                    key={record.recordId}
                                    record={record}
                                    onViewDetail={openRecordDetail}
                                    formatCurrency={formatCurrency}
                                    formatDate={formatDate}
                                    formatTime={formatTime}
                                />
                            ))
                        )}
                    </div>
                </div>
            </div>

            {/* Modal chi tiết hồ sơ */}
            {selectedRecord && selectedAppointment && (
                <MedicalRecordDetailModal
                    show={showDetailModal}
                    onHide={() => {
                        setShowDetailModal(false);
                        setSelectedRecord(null);
                        setSelectedAppointment(null);
                    }}
                    record={selectedRecord}
                    appointment={selectedAppointment}
                    formatCurrency={formatCurrency}
                    formatDate={formatDate}
                />
            )}
        </>
    );
};

// Component hiển thị thẻ hồ sơ
const RecordCard = ({ record, onViewDetail, formatCurrency, formatDate, formatTime }) => {
    return (
        <Card className="mb-4 border-0 shadow-sm hover-shadow">
            <Card.Body className="p-4">
                {/* Header thẻ */}
                <div className="d-flex justify-content-between align-items-start mb-3">
                    <div>
                        <h5 className="fw-bold text-primary mb-1">{record.recordNumber}</h5>
                        <p className="text-muted small mb-0">Ngày tạo: {formatDate(record.issuedDate)}</p>
                    </div>
                    <Badge
                        bg={record.recordStatus === 'Active' ? 'success' : 'secondary'}
                        className="fs-7"
                    >
                        {record.recordStatus === 'Active' ? 'Đang hoạt động' : 'Đã lưu trữ'}
                    </Badge>
                </div>

                {/* Danh sách lịch hẹn */}
                <div className="mt-3">
                    <h6 className="fw-semibold text-dark mb-3">Lịch sử khám:</h6>
                    {record.appointments && record.appointments.map((appointment, index) => (
                        <AppointmentItem
                            key={appointment.appointmentId}
                            appointment={appointment}
                            record={record}
                            onViewDetail={onViewDetail}
                            formatCurrency={formatCurrency}
                            formatDate={formatDate}
                            formatTime={formatTime}
                            isLast={index === record.appointments.length - 1}
                        />
                    ))}
                </div>
            </Card.Body>
        </Card>
    );
};

// Component hiển thị từng lịch hẹn
const AppointmentItem = ({ appointment, record, onViewDetail, formatCurrency, formatDate, formatTime, isLast }) => {
    const getInvoiceStatusVariant = (status) => {
        switch (status) {
            case 'Paid': return 'success';
            case 'Pending': return 'warning';
            default: return 'secondary';
        }
    };

    const getInvoiceStatusText = (status) => {
        switch (status) {
            case 'Paid': return 'Đã thanh toán';
            case 'Pending': return 'Chờ thanh toán';
            default: return status;
        }
    };

    return (
        <div className={`border-start border-3 border-primary ps-3 pb-3 ${!isLast ? 'mb-3' : ''}`}>
            <div className="d-flex justify-content-between align-items-start">
                <div className="flex-grow-1">
                    <div className="d-flex align-items-center mb-1">
                        <h6 className="fw-semibold text-dark mb-0 me-2">
                            {formatDate(appointment.appointmentDate)} lúc {formatTime(appointment.appointmentTime)}
                        </h6>
                        {appointment.invoice && (
                            <Badge bg={getInvoiceStatusVariant(appointment.invoice.status)} className="fs-7">
                                {getInvoiceStatusText(appointment.invoice.status)}
                            </Badge>
                        )}
                    </div>

                    <div className="mb-2">
                        <span className="text-muted small">Bác sĩ: </span>
                        <strong>{appointment.doctorName}</strong>
                        {appointment.doctorSpecialty && (
                            <span className="text-muted small"> - {appointment.doctorSpecialty}</span>
                        )}
                    </div>

                    {appointment.diagnosis && appointment.diagnosis.diagnosis && (
                        <div className="mb-2">
                            <span className="text-muted small">Chuẩn đoán: </span>
                            <span className="fw-medium">{appointment.diagnosis.diagnosis}</span>
                        </div>
                    )}

                    {appointment.invoice && (
                        <div className="mb-2">
                            <span className="text-muted small">Tổng tiền: </span>
                            <strong className="text-success">{formatCurrency(appointment.invoice.totalAmount)}</strong>
                        </div>
                    )}
                </div>

                <Button
                    variant="outline-primary"
                    size="sm"
                    onClick={() => onViewDetail(record, appointment)}
                    className="ms-3 flex-shrink-0"
                >
                    <i className="bi bi-eye me-1"></i>
                    Chi tiết
                </Button>
            </div>
        </div>
    );
};

// Component in hồ sơ - Tách riêng để fix lỗi ref
const PrintableMedicalRecord = React.forwardRef(({ record, appointment, formatCurrency, formatDate }, ref) => {
    const medicines = appointment.prescription?.details || [];
    const services = appointment.services || [];

    return (
        <div ref={ref} className="p-4" style={{ fontSize: '12pt', fontFamily: 'Arial, sans-serif' }}>
            {/* Header */}
            <div className="text-center border-bottom pb-3 mb-4">
                <h4 className="fw-bold mb-1">PHIẾU KHÁM BỆNH</h4>
                <p className="mb-1">BỆNH VIỆN ĐA KHOA MEDPRO</p>
                <p className="small mb-0">123 Nguyễn Văn Linh, Quận 7, TP.HCM - ĐT: (028) 1234 5678</p>
            </div>

            {/* Thông tin cơ bản */}
            <div className="row mb-4">
                <div className="col-6">
                    <table className="table table-bordered" style={{ fontSize: '11pt' }}>
                        <tbody>
                            <tr>
                                <td width="40%" className="fw-bold">Mã hồ sơ:</td>
                                <td>{record.recordNumber}</td>
                            </tr>
                            <tr>
                                <td className="fw-bold">Bệnh nhân:</td>
                                <td>{record.patientName}</td>
                            </tr>
                            <tr>
                                <td className="fw-bold">Ngày khám:</td>
                                <td>{formatDate(appointment.appointmentDate)}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div className="col-6">
                    <table className="table table-bordered" style={{ fontSize: '11pt' }}>
                        <tbody>
                            <tr>
                                <td width="40%" className="fw-bold">Bác sĩ:</td>
                                <td>{appointment.doctorName}</td>
                            </tr>
                            <tr>
                                <td className="fw-bold">Chuyên khoa:</td>
                                <td>{appointment.doctorSpecialty || 'N/A'}</td>
                            </tr>
                            <tr>
                                <td className="fw-bold">Mã hóa đơn:</td>
                                <td>HD-{appointment.invoice?.invoiceId || 'N/A'}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Thông tin khám bệnh */}
            {appointment.diagnosis && (
                <div className="mb-4">
                    <h6 className="fw-bold border-bottom pb-1">THÔNG TIN KHÁM BỆNH</h6>
                    <table className="table table-bordered" style={{ fontSize: '11pt' }}>
                        <tbody>
                            <tr>
                                <td width="20%" className="fw-bold">Triệu chứng:</td>
                                <td>{appointment.diagnosis.symptoms || 'Không có'}</td>
                            </tr>
                            <tr>
                                <td className="fw-bold">Chuẩn đoán:</td>
                                <td>{appointment.diagnosis.diagnosis || 'Chưa có'}</td>
                            </tr>
                            {appointment.diagnosis.notes && (
                                <tr>
                                    <td className="fw-bold">Ghi chú:</td>
                                    <td>{appointment.diagnosis.notes}</td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            )}

            {/* Đơn thuốc */}
            {medicines.length > 0 && (
                <div className="mb-4">
                    <h6 className="fw-bold border-bottom pb-1">ĐƠN THUỐC</h6>
                    {appointment.prescription.instructions && (
                        <p><strong>Hướng dẫn:</strong> {appointment.prescription.instructions}</p>
                    )}
                    <table className="table table-bordered" style={{ fontSize: '10pt' }}>
                        <thead className="table-light">
                            <tr>
                                <th width="5%">STT</th>
                                <th width="30%">Tên thuốc</th>
                                <th width="20%">Loại thuốc</th>
                                <th width="10%">Đơn vị</th>
                                <th width="10%">Số lượng</th>
                                <th width="25%">Hướng dẫn sử dụng</th>
                            </tr>
                        </thead>
                        <tbody>
                            {medicines.map((medicine, index) => (
                                <tr key={index}>
                                    <td>{index + 1}</td>
                                    <td className="fw-bold">{medicine.medicineName}</td>
                                    <td>{medicine.medicineType}</td>
                                    <td>{medicine.unit}</td>
                                    <td>{medicine.quantity}</td>
                                    <td>{medicine.dosageInstruction}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {/* Dịch vụ */}
            {services.length > 0 && (
                <div className="mb-4">
                    <h6 className="fw-bold border-bottom pb-1">DỊCH VỤ ĐÃ SỬ DỤNG</h6>
                    <table className="table table-bordered" style={{ fontSize: '10pt' }}>
                        <thead className="table-light">
                            <tr>
                                <th width="5%">STT</th>
                                <th width="45%">Tên dịch vụ</th>
                                <th width="20%">Loại dịch vụ</th>
                                <th width="15%">Trạng thái</th>
                                <th width="15%" className="text-end">Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            {services.map((service, index) => (
                                <tr key={index}>
                                    <td>{index + 1}</td>
                                    <td>{service.serviceName}</td>
                                    <td>{service.serviceType}</td>
                                    <td>{service.status === 'Completed' ? 'Hoàn thành' : 'Đang xử lý'}</td>
                                    <td className="text-end">{formatCurrency(service.price)}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {/* Hóa đơn */}
            {appointment.invoice && (
                <div className="mb-4">
                    <h6 className="fw-bold border-bottom pb-1">HÓA ĐƠN THANH TOÁN</h6>
                    <table className="table table-bordered" style={{ fontSize: '10pt' }}>
                        <thead className="table-light">
                            <tr>
                                <th width="5%">STT</th>
                                <th width="55%">Mô tả</th>
                                <th width="10%">Số lượng</th>
                                <th width="15%" className="text-end">Đơn giá</th>
                                <th width="15%" className="text-end">Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            {appointment.invoice.details.map((item, index) => (
                                <tr key={index}>
                                    <td>{index + 1}</td>
                                    <td>{item.serviceName}</td>
                                    <td>{item.quantity}</td>
                                    <td className="text-end">{formatCurrency(item.unitPrice)}</td>
                                    <td className="text-end">{formatCurrency(item.subTotal)}</td>
                                </tr>
                            ))}
                        </tbody>
                        <tfoot>
                            <tr>
                                <td colSpan="4" className="text-end fw-bold">Tổng cộng:</td>
                                <td className="text-end fw-bold">{formatCurrency(appointment.invoice.totalAmount)}</td>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            )}

            {/* Chữ ký */}
            <div className="row mt-5">
                <div className="col-6 text-center">
                    <p className="fw-bold mb-4">BÁC SĨ ĐIỀU TRỊ</p>
                    <p className="border-top pt-3">({appointment.doctorName})</p>
                </div>
                <div className="col-6 text-center">
                    <p className="fw-bold mb-4">BỆNH NHÂN</p>
                    <p className="border-top pt-3">({record.patientName})</p>
                </div>
            </div>

            {/* Footer */}
            <div className="text-center mt-4 pt-3 border-top">
                <p className="small text-muted mb-0">
                    Phiếu khám được in từ hệ thống MedPro - Ngày in: {formatDate(new Date())}
                </p>
            </div>
        </div>
    );
});

// Modal hiển thị chi tiết hồ sơ
const MedicalRecordDetailModal = ({ show, onHide, record, appointment, formatCurrency, formatDate }) => {
    const [activeKey, setActiveKey] = useState('0');
    const printRef = useRef();

    // Chức năng in - SỬA LẠI Ở ĐÂY
    const handlePrint = useReactToPrint({
        content: () => printRef.current,
        documentTitle: `Ho-so-benh-an-${record.recordNumber}`,
        pageStyle: `
            @media print {
                body { 
                    -webkit-print-color-adjust: exact; 
                    font-size: 12pt;
                    font-family: 'Times New Roman', serif;
                }
                .no-print { display: none !important; }
                .table { border-collapse: collapse; width: 100%; }
                .table-bordered th, .table-bordered td { border: 1px solid #000 !important; }
                .text-primary { color: #000 !important; }
                .badge { background-color: #f8f9fa !important; color: #000 !important; border: 1px solid #000; }
            }
        `,
    });

    const medicines = appointment.prescription?.details || [];
    const services = appointment.services || [];

    return (
        <>
            <Modal show={show} onHide={onHide} size="lg" centered scrollable className="no-print">
                <Modal.Header closeButton className="bg-light border-bottom">
                    <Modal.Title className="fs-6 text-dark">
                        <i className="bi bi-clipboard2-pulse me-2 text-primary"></i>
                        Chi Tiết Phiếu Khám
                    </Modal.Title>
                </Modal.Header>
                <Modal.Body className="p-0">
                    {/* Thông tin header */}
                    <div className="p-3 bg-light border-bottom">
                        <div className="row g-2">
                            <div className="col-6">
                                <strong className="small text-muted">Mã hồ sơ:</strong>
                                <div className="fw-bold text-primary">{record.recordNumber}</div>
                            </div>
                            <div className="col-6">
                                <strong className="small text-muted">Ngày khám:</strong>
                                <div>{formatDate(appointment.appointmentDate)}</div>
                            </div>
                            <div className="col-6">
                                <strong className="small text-muted">Bác sĩ:</strong>
                                <div className="fw-medium">{appointment.doctorName}</div>
                            </div>
                            <div className="col-6">
                                <strong className="small text-muted">Bệnh nhân:</strong>
                                <div>{record.patientName}</div>
                            </div>
                        </div>
                    </div>

                    <Accordion activeKey={activeKey} onSelect={setActiveKey} flush>
                        {/* Thông tin khám bệnh */}
                        <Accordion.Item eventKey="0" className="border-0">
                            <Accordion.Header className="py-2 px-3">
                                <i className="bi bi-clipboard2-check me-2 text-success"></i>
                                Thông Tin Khám Bệnh
                            </Accordion.Header>
                            <Accordion.Body className="py-3 px-3">
                                {appointment.diagnosis ? (
                                    <div className="row g-2">
                                        <div className="col-12">
                                            <strong className="small text-muted">Triệu chứng:</strong>
                                            <div className="p-2 bg-light rounded small">
                                                {appointment.diagnosis.symptoms || 'Không có thông tin triệu chứng'}
                                            </div>
                                        </div>
                                        <div className="col-12">
                                            <strong className="small text-muted">Chuẩn đoán:</strong>
                                            <div className="p-2 bg-light rounded small">
                                                {appointment.diagnosis.diagnosis || 'Chưa có chuẩn đoán'}
                                            </div>
                                        </div>
                                        {appointment.diagnosis.notes && (
                                            <div className="col-12">
                                                <strong className="small text-muted">Ghi chú:</strong>
                                                <div className="p-2 bg-light rounded small">
                                                    {appointment.diagnosis.notes}
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                ) : (
                                    <div className="text-center text-muted py-2">
                                        <i className="bi bi-clipboard2-x fs-4 d-block mb-1"></i>
                                        <small>Chưa có thông tin khám bệnh</small>
                                    </div>
                                )}
                            </Accordion.Body>
                        </Accordion.Item>

                        {/* Đơn thuốc */}
                        <Accordion.Item eventKey="1" className="border-0">
                            <Accordion.Header className="py-2 px-3">
                                <i className="bi bi-capsule me-2 text-warning"></i>
                                Đơn Thuốc
                                {medicines.length > 0 && (
                                    <Badge bg="warning" text="dark" className="ms-2 fs-7">
                                        {medicines.length} loại
                                    </Badge>
                                )}
                            </Accordion.Header>
                            <Accordion.Body className="py-3 px-3">
                                {medicines.length > 0 ? (
                                    <>
                                        {appointment.prescription.instructions && (
                                            <div className="mb-3 p-2 bg-warning bg-opacity-10 rounded small">
                                                <strong className="text-muted">Hướng dẫn sử dụng:</strong>
                                                <div>{appointment.prescription.instructions}</div>
                                            </div>
                                        )}
                                        <div className="table-responsive">
                                            <Table bordered size="sm" className="mb-0">
                                                <thead className="table-warning">
                                                    <tr>
                                                        <th className="small">Tên thuốc</th>
                                                        <th className="small">Đơn vị</th>
                                                        <th className="small">Số lượng</th>
                                                        <th className="small">Hướng dẫn</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {medicines.map((medicine, index) => (
                                                        <tr key={index}>
                                                            <td className="small">
                                                                <div className="fw-medium">{medicine.medicineName}</div>
                                                                <small className="text-muted">{medicine.medicineType}</small>
                                                            </td>
                                                            <td className="small">{medicine.unit}</td>
                                                            <td className="small">{medicine.quantity}</td>
                                                            <td className="small">{medicine.dosageInstruction}</td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </Table>
                                        </div>
                                    </>
                                ) : (
                                    <div className="text-center text-muted py-2">
                                        <i className="bi bi-capsule fs-4 d-block mb-1"></i>
                                        <small>Không có đơn thuốc</small>
                                    </div>
                                )}
                            </Accordion.Body>
                        </Accordion.Item>

                        {/* Dịch vụ */}
                        <Accordion.Item eventKey="2" className="border-0">
                            <Accordion.Header className="py-2 px-3">
                                <i className="bi bi-heart-pulse me-2 text-danger"></i>
                                Dịch Vụ
                                {services.length > 0 && (
                                    <Badge bg="danger" className="ms-2 fs-7">
                                        {services.length}
                                    </Badge>
                                )}
                            </Accordion.Header>
                            <Accordion.Body className="py-3 px-3">
                                {services.length > 0 ? (
                                    <div className="table-responsive">
                                        <Table bordered size="sm" className="mb-0">
                                            <thead className="table-light">
                                                <tr>
                                                    <th className="small">Tên dịch vụ</th>
                                                    <th className="small">Loại</th>
                                                    <th className="small">Trạng thái</th>
                                                    <th className="small text-end">Thành tiền</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {services.map((service, index) => (
                                                    <tr key={index}>
                                                        <td className="small fw-medium">{service.serviceName}</td>
                                                        <td className="small">{service.serviceType}</td>
                                                        <td className="small">
                                                            <Badge
                                                                bg={service.status === 'Completed' ? 'success' : 'warning'}
                                                                className="fs-7"
                                                            >
                                                                {service.status === 'Completed' ? 'Hoàn thành' : 'Đang xử lý'}
                                                            </Badge>
                                                        </td>
                                                        <td className="small text-end fw-medium">
                                                            {formatCurrency(service.price)}
                                                        </td>
                                                    </tr>
                                                ))}
                                            </tbody>
                                        </Table>
                                    </div>
                                ) : (
                                    <div className="text-center text-muted py-2">
                                        <i className="bi bi-heart-pulse fs-4 d-block mb-1"></i>
                                        <small>Không có dịch vụ</small>
                                    </div>
                                )}
                            </Accordion.Body>
                        </Accordion.Item>

                        {/* Hóa đơn */}
                        <Accordion.Item eventKey="3" className="border-0">
                            <Accordion.Header className="py-2 px-3">
                                <i className="bi bi-receipt me-2 text-info"></i>
                                Hóa Đơn
                            </Accordion.Header>
                            <Accordion.Body className="py-3 px-3">
                                {appointment.invoice ? (
                                    <>
                                        <div className="row g-2 mb-3">
                                            <div className="col-6">
                                                <strong className="small text-muted">Mã hóa đơn:</strong>
                                                <div className="fw-medium">HD-{appointment.invoice.invoiceId}</div>
                                            </div>
                                            <div className="col-6">
                                                <strong className="small text-muted">Trạng thái:</strong>
                                                <div>
                                                    <Badge
                                                        bg={appointment.invoice.status === 'Paid' ? 'success' : 'warning'}
                                                        className="fs-7"
                                                    >
                                                        {appointment.invoice.status === 'Paid' ? 'Đã thanh toán' : 'Chờ thanh toán'}
                                                    </Badge>
                                                </div>
                                            </div>
                                        </div>

                                        <div className="table-responsive">
                                            <Table bordered size="sm" className="mb-2">
                                                <thead className="table-info">
                                                    <tr>
                                                        <th className="small">Mô tả</th>
                                                        <th className="small">Số lượng</th>
                                                        <th className="small text-end">Đơn giá</th>
                                                        <th className="small text-end">Thành tiền</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {appointment.invoice.details.map((item, index) => (
                                                        <tr key={index}>
                                                            <td className="small">{item.serviceName}</td>
                                                            <td className="small">{item.quantity}</td>
                                                            <td className="small text-end">{formatCurrency(item.unitPrice)}</td>
                                                            <td className="small text-end fw-medium">{formatCurrency(item.subTotal)}</td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </Table>
                                        </div>

                                        <div className="text-end border-top pt-2">
                                            <strong className="text-dark">Tổng cộng: </strong>
                                            <strong className="text-success fs-5">
                                                {formatCurrency(appointment.invoice.totalAmount)}
                                            </strong>
                                        </div>
                                    </>
                                ) : (
                                    <div className="text-center text-muted py-2">
                                        <i className="bi bi-receipt fs-4 d-block mb-1"></i>
                                        <small>Chưa có hóa đơn</small>
                                    </div>
                                )}
                            </Accordion.Body>
                        </Accordion.Item>
                    </Accordion>
                </Modal.Body>
                <Modal.Footer className="no-print bg-light border-top">
                    <Button variant="outline-secondary" onClick={onHide} size="sm">
                        Đóng
                    </Button>
                    <Button variant="primary" onClick={handlePrint} size="sm">
                        <i className="bi bi-printer me-1"></i>
                        In phiếu khám
                    </Button>
                </Modal.Footer>
            </Modal>

            {/* Component in - ĐẶT NGOÀI MODAL VÀ CHỈ HIỆN KHI MODAL HIỆN */}
            {show && (
                <div style={{ position: 'absolute', left: '-10000px', top: 'auto', width: '1px', height: '1px', overflow: 'hidden' }}>
                    <PrintableMedicalRecord
                        ref={printRef}
                        record={record}
                        appointment={appointment}
                        formatCurrency={formatCurrency}
                        formatDate={formatDate}
                    />
                </div>
            )}
        </>
    );
};

export default PatientMedicalHistory;