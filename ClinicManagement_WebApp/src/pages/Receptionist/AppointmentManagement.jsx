import React, { useState, useEffect, useCallback } from 'react';
import { Table, Badge, Spinner, Alert, Form, Row, Col, Button, Dropdown } from 'react-bootstrap';
import Pagination from '../../Components/Pagination/Pagination';
import instance from '../../axios';
import CustomToast from '../../Components/CustomToast/CustomToast';
import { Filter, X, Users } from 'lucide-react';

const AppointmentManagement = () => {
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState(null);

  // Bộ lọc
  const [staffId, setStaffId] = useState('');
  const [selectedDate, setSelectedDate] = useState('');

  // Phân trang
  const [currentPage, setCurrentPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 20;

  // Danh sách trạng thái
  const statusList = [
    { value: 'Ordered',     label: 'Đã đặt',      color: 'secondary' },
    { value: 'Waiting',     label: 'Đang chờ',    color: 'warning' },
    { value: 'InProgress',  label: 'Đang khám',   color: 'primary' },
    { value: 'Completed',   label: 'Hoàn thành',  color: 'success' },
    { value: 'Cancelled',   label: 'Đã hủy',      color: 'danger' }
  ];

  const getBadge = (status) => {
    const s = statusList.find(item => item.value === status);
    return s ? <Badge bg={s.color}>{s.label}</Badge> : <Badge bg="secondary">—</Badge>;
  };

  // Hàm lấy dữ liệu - có phân trang + filter
  const fetchAppointments = useCallback(async (page = 1) => {
    setLoading(true);
    try {
      let url = 'Appointment';
      const params = {
        page,
        pageSize,
        sortBy: 'appointmentDateTime',
        sortOrder: 'desc'
      };

      if (staffId.trim()) {
        url = `Appointment/staff/${staffId.trim()}`;
      }
      if (selectedDate) {
        params.date = selectedDate;
      }

      const res = await instance.get(url, { params });
      const response = res.data;

      const data = response.data || response.content || response.items || response || [];
      const total = response.totalItems || response.totalCount || response.total || data.length;

      // Sắp xếp lại theo ngày giờ (đảm bảo mới nhất lên đầu)
      const sorted = [...data].sort((a, b) =>
        new Date(`${b.appointmentDate} ${b.appointmentTime || ''}`) -
        new Date(`${a.appointmentDate} ${a.appointmentTime || ''}`)
      );

      setAppointments(sorted);
      setTotalItems(total);
      setTotalPages(Math.ceil(total / pageSize));
      setCurrentPage(page);

    } catch (err) {
      console.error('Lỗi tải lịch hẹn:', err);
      setToast({ type: 'error', message: 'Không tải được danh sách lịch hẹn' });
    } finally {
      setLoading(false);
    }
  }, [staffId, selectedDate]);

  // Khi filter thay đổi → reset về trang 1
  useEffect(() => {
    setCurrentPage(1);
  }, [staffId, selectedDate]);

  // Khi currentPage thay đổi → load dữ liệu trang đó
  useEffect(() => {
    fetchAppointments(currentPage);
  }, [currentPage, fetchAppointments]);

  // Xử lý chuyển trang
  const handlePageChange = ({ selected }) => {
    setCurrentPage(selected + 1);
  };

  // Xóa bộ lọc
  const clearFilters = () => {
    setStaffId('');
    setSelectedDate('');
    // currentPage sẽ tự động về 1 nhờ useEffect trên
  };

  // Cập nhật trạng thái
  const updateStatus = async (id, newStatus) => {
    try {
      await instance.put(`/Appointment/AppointmentUpdateStatusAsync/${id}`, { status: newStatus });
      setToast({ type: 'success', message: 'Cập nhật trạng thái thành công!' });
      fetchAppointments(currentPage); // Refresh trang hiện tại
    } catch (err) {
      setToast({ type: 'error', message: 'Cập nhật thất bại!' });
    }
  };

  return (
    <div className="d-flex w-100 bg-light" style={{ minHeight: '100vh' }}>
      <main className="flex-grow-1 p-4">
        {toast && <CustomToast type={toast.type} message={toast.message} onClose={() => setToast(null)} />}

        <div className="bg-white rounded shadow p-4">
          {/* Header */}
          <div className="d-flex justify-content-between align-items-center mb-4">
            <h3 className="text-primary fw-bold m-0">
              <Users className="me-2" size={28} /> Quản Lý Lịch Hẹn
            </h3>
            <div className="d-flex align-items-center gap-3">
              <Badge bg="info" pill className="fs-5 px-4">
                {totalItems} lịch hẹn
              </Badge>
              <small className="text-muted">
                Trang {currentPage} / {totalPages || 1}
              </small>
            </div>
          </div>

          {/* Filter */}
          <div className="bg-light rounded border p-3 mb-4">
            <Row className="g-3 align-items-end">
              <Col md={5}>
                <Form.Label className="fw-bold">Mã bác sĩ</Form.Label>
                <Form.Control
                  placeholder="Để trống = tất cả bác sĩ"
                  value={staffId}
                  onChange={e => setStaffId(e.target.value.replace(/\D/g, ''))}
                />
              </Col>
              <Col md={4}>
                <Form.Label className="fw-bold">Ngày khám</Form.Label>
                <Form.Control type="date" value={selectedDate} onChange={e => setSelectedDate(e.target.value)} />
              </Col>
              <Col md={3}>
                <div className="d-flex gap-2">
                  <Button variant="primary" onClick={() => setCurrentPage(1)} disabled={loading}>
                    <Filter size={16} className="me-1" /> Tìm
                  </Button>
                  <Button variant="outline-secondary" onClick={clearFilters}>
                    <X size={16} className="me-1" /> Xóa lọc
                  </Button>
                </div>
              </Col>
            </Row>
          </div>

          {/* Bảng + Pagination */}
          {loading ? (
            <div className="text-center py-5">
              <Spinner animation="border" variant="primary" size="lg" />
              <p className="mt-3 text-muted">Đang tải lịch hẹn trang {currentPage}...</p>
            </div>
          ) : appointments.length === 0 ? (
            <Alert variant="warning" className="text-center py-5">
              Không tìm thấy lịch hẹn nào
            </Alert>
          ) : (
            <>
              <div className="table-responsive">
                <Table striped bordered hover className="align-middle text-center mb-4">
                  <thead className="table-primary">
                    <tr>
                      <th width="60">STT</th>
                      <th>Mã BN</th>
                      <th>Họ tên</th>
                      <th>Bác sĩ</th>
                      <th>Ngày</th>
                      <th>Giờ</th>
                      <th>Ghi chú</th>
                      <th>Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {appointments.map((apt, idx) => (
                      <tr key={apt.appointmentId}>
                        <td>{(currentPage - 1) * pageSize + idx + 1}</td>
                        <td className="fw-bold text-primary">{apt.patientId}</td>
                        <td>{apt.patientName || '—'}</td>
                        <td>{apt.staffName || apt.doctorName || '—'}</td>
                        <td>{apt.appointmentDate}</td>
                        <td><Badge bg="dark">{apt.appointmentTime?.slice(0, 5) || '—'}</Badge></td>
                        <td className="text start text-truncate" style={{ maxWidth: '200px' }}>
                          {apt.notes || '—'}
                        </td>
                        <td>
                          <Dropdown onSelect={(status) => updateStatus(apt.appointmentId, status)}>
                            <Dropdown.Toggle variant="outline-secondary" size="sm" className="w-100">
                              {getBadge(apt.status)}
                            </Dropdown.Toggle>
                            <Dropdown.Menu>
                              {statusList.map(s => (
                                <Dropdown.Item key={s.value} eventKey={s.value}>
                                  <Badge bg={s.color}>{s.label}</Badge> {s.label}
                                </Dropdown.Item>
                              ))}
                            </Dropdown.Menu>
                          </Dropdown>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="d-flex justify-content-center">
                  <Pagination
                    pageCount={totalPages}
                    currentPage={currentPage - 1}
                    onPageChange={handlePageChange}
                  />
                </div>
              )}
            </>
          )}
        </div>
      </main>
    </div>
  );
};

export default AppointmentManagement;