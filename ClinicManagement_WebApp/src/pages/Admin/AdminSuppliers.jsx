import React, { useState, useEffect, useCallback, useRef, memo } from "react";
import { Table, Button, Spinner, Form, Row, Col, Modal, Dropdown } from "react-bootstrap";
import Pagination from "../../Components/Pagination/Pagination";
import CustomToast from "../../Components/CustomToast/CustomToast";
import { Filter, X, Plus } from "lucide-react";

const API_BASE_URL = "http://localhost:5066";

// ========== StatusDropdown (Tách riêng) ==========
const StatusDropdown = memo(({ appointmentId, currentStatus, onUpdate }) => {
  const statusMap = {
    Pending: "Chờ xác nhận",
    Confirmed: "Đã xác nhận",
    Completed: "Hoàn thành",
    Waiting: "Đang chờ"
  };

  const variantMap = {
    Confirmed: "success",
    Pending: "warning",
    Completed: "primary",
    Waiting: "info"
  };

  return (
    <Dropdown onSelect={(status) => onUpdate(appointmentId, status)}>
      <Dropdown.Toggle
        variant={variantMap[currentStatus] || "secondary"}
        size="sm"
        id={`status-${appointmentId}`}
        className="w-100"
      >
        {statusMap[currentStatus] || currentStatus}
      </Dropdown.Toggle>

      <Dropdown.Menu>
        {Object.entries(statusMap).map(([key, label]) => (
          <Dropdown.Item key={key} eventKey={key}>
            {label}
          </Dropdown.Item>
        ))}
      </Dropdown.Menu>
    </Dropdown>
  );
});

// ========== CreateAppointmentModal (Tách riêng) ==========
const CreateAppointmentModal = memo(({ show, onHide, onSuccess }) => {
  const [form, setForm] = useState({
    patientName: "",
    staffId: "",
    appointmentDate: "",
    appointmentTime: "",
    notes: ""
  });

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await fetch(`${API_BASE_URL}/api/Appointment`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(form)
      });

      if (!response.ok) throw new Error("Tạo thất bại");

      onSuccess("Tạo lịch hẹn thành công!");
      onHide();
      setForm({
        patientName: "",
        staffId: "",
        appointmentDate: "",
        appointmentTime: "",
        notes: ""
      });
    } catch (error) {
      onSuccess("error", error.message);
    }
  };

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Tạo Lịch Hẹn Mới</Modal.Title>
      </Modal.Header>
      <Form onSubmit={handleSubmit}>
        <Modal.Body>
          <Row>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>Tên bệnh nhân</Form.Label>
                <Form.Control
                  value={form.patientName}
                  onChange={(e) => setForm({ ...form, patientName: e.target.value })}
                  required
                />
              </Form.Group>
            </Col>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>Staff ID</Form.Label>
                <Form.Control
                  value={form.staffId}
                  onChange={(e) => setForm({ ...form, staffId: e.target.value.replace(/\D/g, '') })}
                  required
                />
              </Form.Group>
            </Col>
          </Row>
          <Row>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>Ngày</Form.Label>
                <Form.Control
                  type="date"
                  value={form.appointmentDate}
                  onChange={(e) => setForm({ ...form, appointmentDate: e.target.value })}
                  required
                />
              </Form.Group>
            </Col>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>Giờ</Form.Label>
                <Form.Control
                  type="time"
                  value={form.appointmentTime}
                  onChange={(e) => setForm({ ...form, appointmentTime: e.target.value })}
                  required
                />
              </Form.Group>
            </Col>
          </Row>
          <Form.Group className="mb-3">
            <Form.Label>Ghi chú</Form.Label>
            <Form.Control
              as="textarea"
              rows={2}
              value={form.notes}
              onChange={(e) => setForm({ ...form, notes: e.target.value })}
            />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={onHide}>Hủy</Button>
          <Button variant="primary" type="submit">Tạo</Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
});

// ========== AppointmentList ==========
const AppointmentList = memo(({
  appointments,
  isLoading,
  applyFilters,
  clearFilters,
  filters,
  setFilters,
  pageCount,
  currentPage,
  handlePageChange,
  onUpdateStatus,
  onCancel,
  onCreate
}) => {
  return (
    <div>
      {/* HEADER */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h3 style={{ fontSize: "1.5rem", fontWeight: "600" }}>Danh Sách Lịch Hẹn</h3>
        <Button variant="success" size="sm" onClick={onCreate}>
          <Plus size={16} /> Tạo mới
        </Button>
      </div>

      {/* FILTER BAR */}
      <div className="mb-4 p-3 bg-light rounded border">
        <Row className="g-3 align-items-center">
          <Col md={5}>
            <Form.Control
              placeholder="Nhập Staff ID..."
              value={filters.staffId}
              onChange={(e) => {
                const value = e.target.value.replace(/\D/g, '');
                setFilters({ ...filters, staffId: value });
              }}
            />
          </Col>
          <Col md={5}>
            <Form.Control
              type="date"
              value={filters.date}
              onChange={(e) => setFilters({ ...filters, date: e.target.value })}
            />
          </Col>
          <Col md={2} className="d-flex gap-1">
            <Button
              variant="primary"
              size="sm"
              onClick={applyFilters}
              className="flex-fill"
              disabled={!filters.staffId}
            >
              <Filter size={16} />
            </Button>
            <Button variant="outline-secondary" size="sm" onClick={clearFilters} className="flex-fill">
              <X size={16} />
            </Button>
          </Col>
        </Row>
      </div>

      {/* TABLE */}
      <div className="table-responsive">
        <Table striped bordered hover responsive className={isLoading ? "opacity-50" : ""}>
          <thead className="table-light">
            <tr>
              <th>ID</th>
              <th>Bệnh nhân</th>
              <th>Bác sĩ</th>
              <th>Ngày</th>
              <th>Giờ</th>
              <th>Trạng thái</th>
              <th>Ghi chú</th>
              <th>Hành động</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr><td colSpan={8} className="text-center py-4"><Spinner animation="border" /></td></tr>
            ) : appointments.length === 0 ? (
              <tr><td colSpan={8} className="text-center py-4 text-muted">
                {!filters.staffId ? "Nhập Staff ID để xem lịch hẹn." : "Không có dữ liệu"}
              </td></tr>
            ) : (
              appointments.map((item) => (
                <tr key={item.appointmentId}>
                  <td>{item.appointmentId}</td>
                  <td>{item.patientName}</td>
                  <td>{item.staffName}</td>
                  <td>{item.appointmentDate}</td>
                  <td>{item.appointmentTime}</td>
                  <td>
                    <StatusDropdown
                      appointmentId={item.appointmentId}
                      currentStatus={item.status}
                      onUpdate={onUpdateStatus}
                    />
                  </td>
                  <td title={item.notes}>
                    {item.notes?.length > 30 ? `${item.notes.substring(0,30)}...` : item.notes || "—"}
                  </td>
                  <td className="text-center">
                    {item.status !== "Cancelled" && item.status !== "Completed" && (
                      <Button
                        variant="link"
                        size="sm"
                        className="text-danger p-0"
                        onClick={() => onCancel(item.appointmentId)}
                        title="Hủy lịch"
                      >
                        <X size={18} />
                      </Button>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </Table>
      </div>

      {/* PAGINATION */}
      {pageCount > 1 && (
        <Pagination
          pageCount={pageCount}
          onPageChange={handlePageChange}
          currentPage={currentPage}
          isLoading={isLoading}
        />
      )}
    </div>
  );
});

// ========== AdminAppointments ==========
const AdminAppointments = () => {
  const [appointments, setAppointments] = useState([]);
  const [currentPage, setCurrentPage] = useState(0);
  const [pageCount, setPageCount] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [toast, setToast] = useState({ show: false, type: "info", message: "" });
  const [filters, setFilters] = useState({ date: "", staffId: "" });
  const [filterParams, setFilterParams] = useState("");
  const [showCreateModal, setShowCreateModal] = useState(false);
  const cache = useRef(new Map());

  const showToast = useCallback((type, message) => {
    setToast({ show: true, type, message });
  }, []);

  const hideToast = useCallback(() => {
    setToast({ show: false, type: "info", message: "" });
  }, []);

  // FETCH
  const fetchAppointments = useCallback(async (page = 1, queryString = "") => {
    if (!filters.staffId) {
      setAppointments([]);
      setPageCount(0);
      return;
    }

    const cacheKey = `${filters.staffId}_${page}_${queryString}`;
    if (cache.current.has(cacheKey)) {
      const { data, totalPages } = cache.current.get(cacheKey);
      setAppointments(data);
      setPageCount(totalPages);
      setCurrentPage(page - 1);
      return;
    }

    try {
      setIsLoading(true);
      const url = `${API_BASE_URL}/api/Appointment/staff/${filters.staffId}?page=${page}${queryString ? "&" + queryString : ""}`;
      const response = await fetch(url, { headers: { Accept: "application/json" } });
      if (!response.ok) throw new Error(`HTTP ${response.status}`);

      const res = await response.json();
      const list = Array.isArray(res.data) ? res.data : [];
      const totalPages = res.totalPages || 1;

      cache.current.set(cacheKey, { data: list, totalPages });
      setAppointments(list);
      setPageCount(totalPages);
      setCurrentPage(page - 1);
    } catch (error) {
      showToast("error", `Lỗi tải dữ liệu: ${error.message}`);
      setAppointments([]);
    } finally {
      setIsLoading(false);
    }
  }, [filters.staffId, showToast]);

  // CẬP NHẬT TRẠNG THÁI
  const handleUpdateStatus = useCallback(async (id, newStatus) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Appointment/AppointmentUpdateStatusAsync/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ status: newStatus })
      });

      if (!response.ok) throw new Error("Cập nhật thất bại");

      showToast("success", "Cập nhật trạng thái thành công!");
      fetchAppointments(currentPage + 1, filterParams);
    } catch (error) {
      showToast("error", `Lỗi: ${error.message}`);
    }
  }, [currentPage, filterParams, fetchAppointments, showToast]);

  // HỦY LỊCH
  const handleCancel = useCallback(async (id) => {
    if (!window.confirm("Xác nhận hủy lịch hẹn này?")) return;

    try {
      const response = await fetch(`${API_BASE_URL}/api/Appointment/AppointmentCancelAsync/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" }
      });

      if (!response.ok) throw new Error("Hủy thất bại");

      showToast("success", "Hủy lịch hẹn thành công!");
      fetchAppointments(currentPage + 1, filterParams);
    } catch (error) {
      showToast("error", `Lỗi: ${error.message}`);
    }
  }, [currentPage, filterParams, fetchAppointments, showToast]);

  // MỞ MODAL TẠO
  const handleOpenCreate = () => setShowCreateModal(true);
  const handleCloseCreate = () => setShowCreateModal(false);

  // ÁP DỤNG LỌC
  const applyFilters = useCallback(() => {
    if (!filters.staffId) {
      showToast("error", "Vui lòng nhập Staff ID!");
      return;
    }

    const params = new URLSearchParams();
    if (filters.date) params.append("date", filters.date);

    const query = params.toString();
    setFilterParams(query);
    fetchAppointments(1, query);
  }, [filters, fetchAppointments, showToast]);

  // XÓA LỌC
  const clearFilters = useCallback(() => {
    setFilters({ date: "", staffId: filters.staffId });
    setFilterParams("");
    if (filters.staffId) fetchAppointments(1);
  }, [filters.staffId, fetchAppointments]);

  // ĐỔI TRANG
  const handlePageChange = useCallback(({ selected }) => {
    fetchAppointments(selected + 1, filterParams);
  }, [fetchAppointments, filterParams]);

  // LOAD KHI NHẬP STAFF ID
  useEffect(() => {
    if (filters.staffId) {
      fetchAppointments(1);
    } else {
      setAppointments([]);
      setPageCount(0);
    }
  }, [filters.staffId, fetchAppointments]);

  return (
    <div className="p-4">
      <AppointmentList
        appointments={appointments}
        isLoading={isLoading}
        applyFilters={applyFilters}
        clearFilters={clearFilters}
        filters={filters}
        setFilters={setFilters}
        pageCount={pageCount}
        currentPage={currentPage}
        handlePageChange={handlePageChange}
        onUpdateStatus={handleUpdateStatus}
        onCancel={handleCancel}
        onCreate={handleOpenCreate}
      />

      {/* MODAL TẠO MỚI */}
      <CreateAppointmentModal
        show={showCreateModal}
        onHide={handleCloseCreate}
        onSuccess={(type, msg) => {
          showToast(type, msg);
          if (type === "success") fetchAppointments(1);
        }}
      />

      {toast.show && (
        <CustomToast type={toast.type} message={toast.message} onClose={hideToast} />
      )}
    </div>
  );
};

export default AdminAppointments;