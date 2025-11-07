// src/pages/AppointmentList.jsx
import React, { useState, useEffect, useCallback, useRef } from "react";
import { Table, Button, Spinner, Form, Row, Col, Dropdown } from "react-bootstrap";
import Pagination from "../../Components/Pagination/Pagination";
import CustomToast from "../../Components/CustomToast/CustomToast";
import { Filter, X } from "lucide-react";

const API_BASE_URL = "http://localhost:5066";

const AppointmentDashboard = () => {
  const [appointments, setAppointments] = useState([]);
  const [currentPage, setCurrentPage] = useState(0);
  const [pageCount, setPageCount] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [toast, setToast] = useState({ show: false, type: "info", message: "" });
  const [filters, setFilters] = useState({ date: "", staffId: "" });
  const [filterParams, setFilterParams] = useState("");
  const cache = useRef(new Map());

  const showToast = useCallback((type, message) => setToast({ show: true, type, message }), []);
  const hideToast = useCallback(() => setToast({ show: false, type: "info", message: "" }), []);

  const fetchAppointments = useCallback(async (page = 1, queryString = "") => {
    if (!filters.staffId) {
      setAppointments([]); setPageCount(0); return;
    }
    const cacheKey = `${filters.staffId}_${page}_${queryString}`;
    if (cache.current.has(cacheKey)) {
      const { data, totalPages } = cache.current.get(cacheKey);
      setAppointments(data); setPageCount(totalPages); setCurrentPage(page - 1); return;
    }

    try {
      setIsLoading(true);
      const url = `${API_BASE_URL}/api/Appointment/staff/${filters.staffId}?page=${page}${queryString ? "&" + queryString : ""}`;
      const res = await fetch(url, { headers: { Accept: "application/json" } });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const json = await res.json();
      const list = Array.isArray(json.data) ? json.data : [];
      const totalPages = json.totalPages || 1;
      cache.current.set(cacheKey, { data: list, totalPages });
      setAppointments(list); setPageCount(totalPages); setCurrentPage(page - 1);
    } catch (error) {
      showToast("error", `Lỗi: ${error.message}`);
      setAppointments([]);
    } finally {
      setIsLoading(false);
    }
  }, [filters.staffId, showToast]);

  const handleUpdateStatus = async (id, status) => {
    try {
      const res = await fetch(`${API_BASE_URL}/api/Appointment/AppointmentUpdateStatusAsync/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ status })
      });
      if (!res.ok) throw new Error("Cập nhật thất bại");
      showToast("success", "Cập nhật thành công!");
      fetchAppointments(currentPage + 1, filterParams);
    } catch (err) {
      showToast("error", err.message);
    }
  };

  const handleCancel = async (id) => {
    if (!window.confirm("Hủy lịch hẹn này?")) return;
    try {
      const res = await fetch(`${API_BASE_URL}/api/Appointment/AppointmentCancelAsync/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" }
      });
      if (!res.ok) throw new Error("Hủy thất bại");
      showToast("success", "Đã hủy!");
      fetchAppointments(currentPage + 1, filterParams);
    } catch (err) {
      showToast("error", err.message);
    }
  };

  const applyFilters = () => {
    if (!filters.staffId) return showToast("error", "Nhập Staff ID!");
    const params = new URLSearchParams();
    if (filters.date) params.append("date", filters.date);
    setFilterParams(params.toString());
    fetchAppointments(1, params.toString());
  };

  const clearFilters = () => {
    setFilters({ date: "", staffId: filters.staffId });
    setFilterParams("");
    if (filters.staffId) fetchAppointments(1);
  };

  const handlePageChange = ({ selected }) => fetchAppointments(selected + 1, filterParams);

  useEffect(() => {
    if (filters.staffId) fetchAppointments(1);
    else { setAppointments([]); setPageCount(0); }
  }, [filters.staffId, fetchAppointments]);

  // DROPDOWN TRẠNG THÁI – GỘP TRỰC TIẾP TRONG FILE
  const StatusDropdown = ({ appointmentId, currentStatus }) => {
    const statusMap = {
      Pending: "Chờ xác nhận",
      Confirmed: "Đã xác nhận",
      Completed: "Hoàn thành",
      Waiting: "Đang chờ"
    };
    const variantMap = { Confirmed: "success", Pending: "warning", Completed: "primary", Waiting: "info" };

    return (
      <Dropdown onSelect={(status) => handleUpdateStatus(appointmentId, status)}>
        <Dropdown.Toggle
          variant={variantMap[currentStatus] || "secondary"}
          size="sm"
          className="w-100"
        >
          {statusMap[currentStatus] || currentStatus}
        </Dropdown.Toggle>
        <Dropdown.Menu>
          {Object.entries(statusMap).map(([key, label]) => (
            <Dropdown.Item key={key} eventKey={key}>{label}</Dropdown.Item>
          ))}
        </Dropdown.Menu>
      </Dropdown>
    );
  };

  return (
    <div className="p-4">
      <h3 className="mb-4">Danh Sách Lịch Hẹn</h3>

      {/* FILTER */}
      <div className="mb-4 p-3 bg-light rounded border">
        <Row className="g-3 align-items-center">
          <Col md={5}>
            <Form.Control
              placeholder="Staff ID..."
              value={filters.staffId}
              onChange={e => setFilters({ ...filters, staffId: e.target.value.replace(/\D/g, '') })}
            />
          </Col>
          <Col md={5}>
            <Form.Control
              type="date"
              value={filters.date}
              onChange={e => setFilters({ ...filters, date: e.target.value })}
            />
          </Col>
          <Col md={2} className="d-flex gap-1">
            <Button variant="primary" size="sm" onClick={applyFilters} disabled={!filters.staffId}>
              <Filter size={16} />
            </Button>
            <Button variant="outline-secondary" size="sm" onClick={clearFilters}>
              <X size={16} />
            </Button>
          </Col>
        </Row>
      </div>

      {/* TABLE */}
      <div className="table-responsive">
        <Table striped bordered hover>
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
              <tr><td colSpan={8} className="text-center"><Spinner animation="border" /></td></tr>
            ) : appointments.length === 0 ? (
              <tr><td colSpan={8} className="text-center text-muted">
                {!filters.staffId ? "Nhập Staff ID để xem." : "Không có dữ liệu"}
              </td></tr>
            ) : (
              appointments.map(item => (
                <tr key={item.appointmentId}>
                  <td>{item.appointmentId}</td>
                  <td>{item.patientName}</td>
                  <td>{item.staffName}</td>
                  <td>{item.appointmentDate}</td>
                  <td>{item.appointmentTime}</td>
                  <td><StatusDropdown appointmentId={item.appointmentId} currentStatus={item.status} /></td>
                  <td title={item.notes}>{item.notes?.substring(0,30) || "—"}</td>
                  <td className="text-center">
                    {item.status !== "Cancelled" && item.status !== "Completed" && (
                      <Button variant="link" className="text-danger p-0" onClick={() => handleCancel(item.appointmentId)}>
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

      {pageCount > 1 && <Pagination pageCount={pageCount} onPageChange={handlePageChange} currentPage={currentPage} isLoading={isLoading} />}
      {toast.show && <CustomToast type={toast.type} message={toast.message} onClose={hideToast} />}
    </div>
  );
};

export default AppointmentDashboard;