import React, { useState, useEffect } from 'react';
import { Form, Button, Modal, Spinner, Alert, Badge } from 'react-bootstrap';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';
import { vi } from 'date-fns/locale';
import { useLocation, useNavigate } from 'react-router-dom';
import instance from '../../axios';
import CustomToast from '../../Components/CustomToast/CustomToast';

const AppointmentUpdate = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const appointmentId = location.state?.appointmentId;

  const [appointment, setAppointment] = useState(null);
  const [staffList, setStaffList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [toast, setToast] = useState(null);

  // Form data
  const [formData, setFormData] = useState({
    staffId: '',
    appointmentTime: null,
    notes: ''
  });

  // Modal hủy
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');

  // Lấy dữ liệu lịch hẹn + danh sách bác sĩ
  useEffect(() => {
    if (!appointmentId) {
      setToast({ type: 'error', message: 'Không tìm thấy lịch hẹn!' });
      navigate('/receptionist/appointment-management');
      return;
    }

    const fetchData = async () => {
      try {
        const [apptRes, staffRes] = await Promise.all([
          instance.get(`Appointment/${appointmentId}`),
          instance.get('Admin/GetAllMedicalStaffAsync')
        ]);

        const appt = apptRes.data || apptRes.content;
        setAppointment(appt);

        // Gán dữ liệu vào form
        const apptDateTime = new Date(`${appt.appointmentDate}T${appt.appointmentTime}`);
        setFormData({
          staffId: appt.staffId,
          appointmentTime: apptDateTime,
          notes: appt.notes || ''
        });

        setStaffList(staffRes.data || staffRes.content || []);
      } catch (err) {
        setToast({ type: 'error', message: 'Không tải được thông tin lịch hẹn.' });
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [appointmentId, navigate]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleDateChange = (date) => {
    setFormData(prev => ({ ...prev, appointmentTime: date }));
  };

  // Cập nhật lịch hẹn
  const handleUpdate = async (e) => {
    e.preventDefault();
    if (!formData.appointmentTime) return;

    setSaving(true);
    const selectedDate = formData.appointmentTime;
    const payload = {
      staffId: Number(formData.staffId),
      appointmentDate: selectedDate.toISOString().split('T')[0],
      appointmentTime: selectedDate.toTimeString().slice(0, 8),
      notes: formData.notes.trim()
    };

    try {
      await instance.put(`Appointment/${appointmentId}`, payload);
      setToast({ type: 'success', message: 'Cập nhật lịch hẹn thành công!' });
      setTimeout(() => navigate('/receptionist/appointment-management'), 1500);
    } catch (err) {
      setToast({ type: 'error', message: 'Cập nhật thất bại. Vui lòng thử lại.' });
    } finally {
      setSaving(false);
    }
  };

  // Hủy lịch hẹn
  const handleCancel = async () => {
    if (!cancelReason.trim()) {
      setToast({ type: 'warning', message: 'Vui lòng nhập lý do hủy!' });
      return;
    }

    try {
      await instance.put(`Appointment/${appointmentId}/cancel`, {
        cancelReason: cancelReason.trim()
      });
      setToast({ type: 'success', message: 'Đã hủy lịch hẹn thành công!' });
      setShowCancelModal(false);
      setTimeout(() => navigate('/receptionist/appointment-management'), 1500);
    } catch (err) {
      setToast({ type: 'error', message: 'Hủy lịch thất bại.' });
    }
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '70vh' }}>
        <Spinner animation="border" variant="primary" />
      </div>
    );
  }

  if (!appointment) return null;

  return (
    <main className="main-content flex-grow-1 p-4">
      {toast && <CustomToast type={toast.type} message={toast.message} onClose={() => setToast(null)} />}

      <div className="bg-white rounded shadow-sm p-4" style={{ maxWidth: '800px', margin: '0 auto' }}>
        <h3 className="mb-4 text-primary fw-bold text-center">
          <i className="fas fa-edit me-2"></i>
          Cập Nhật / Hủy Lịch Hẹn
        </h3>

        {/* Thông tin bệnh nhân */}
        <Alert variant="info" className="mb-4">
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <strong>Mã BN:</strong> {appointment.patientId || 'N/A'} <br />
              <strong>Họ tên:</strong> {appointment.patientName || 'Chưa có thông tin'}
            </div>
            <Badge bg={appointment.status === 'Pending' ? 'warning' : appointment.status === 'Completed' ? 'success' : 'secondary'}>
              {appointment.status === 'Pending' ? 'Chờ khám' : appointment.status === 'Completed' ? 'Đã khám' : 'Đã hủy'}
            </Badge>
          </div>
        </Alert>

        <Form onSubmit={handleUpdate}>
          {/* Bác sĩ */}
          <Form.Group className="mb-3">
            <Form.Label>Bác Sĩ Phụ Trách</Form.Label>
            <Form.Select name="staffId" value={formData.staffId} onChange={handleChange} required>
              <option value="">Chọn bác sĩ</option>
              {staffList.map(s => (
                <option key={s.staffId} value={s.staffId}>
                  {s.staffName} ({s.staffType})
                </option>
              ))}
            </Form.Select>
          </Form.Group>

          {/* Thời gian khám */}
          <Form.Group className="mb-3">
            <Form.Label>Thời Gian Khám Mới</Form.Label>
            <DatePicker
              selected={formData.appointmentTime}
              onChange={handleDateChange}
              showTimeSelect
              timeFormat="HH:mm"
              timeIntervals={30}
              dateFormat="dd/MM/yyyy - HH:mm"
              locale={vi}
              className="form-control"
              placeholderText="Chọn ngày giờ mới"
              minDate={new Date()}
              required
            />
          </Form.Group>

          {/* Lý do khám */}
          <Form.Group className="mb-4">
            <Form.Label>Lý Do Khám (Cập nhật)</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              name="notes"
              value={formData.notes}
              onChange={handleChange}
              placeholder="Cập nhật triệu chứng, yêu cầu..."
            />
          </Form.Group>

          {/* Nút hành động */}
          <div className="d-flex gap-3">
            <Button
              type="submit"
              variant="primary"
              className="flex-fill rounded-pill"
              disabled={saving}
            >
              {saving ? 'Đang cập nhật...' : 'Cập Nhật Lịch'}
            </Button>

            <Button
              variant="danger"
              className="flex-fill rounded-pill"
              onClick={() => setShowCancelModal(true)}
              disabled={saving}
            >
              Hủy Lịch Hẹn
            </Button>

            <Button
              variant="secondary"
              className="flex-fill rounded-pill"
              onClick={() => navigate('/receptionist/appointment-management')}
            >
              Quay Lại
            </Button>
          </div>
        </Form>
      </div>

      {/* Modal xác nhận hủy */}
      <Modal show={showCancelModal} onHide={() => setShowCancelModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title className="text-danger">
            <i className="fas fa-exclamation-triangle me-2"></i>
            Xác Nhận Hủy Lịch
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p>Bạn có chắc chắn muốn <strong>hủy lịch hẹn</strong> này?</p>
          <Form.Group>
            <Form.Label>Lý do hủy <span className="text-danger">(bắt buộc)</span></Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              placeholder="VD: Bệnh nhân không đến, yêu cầu dời lịch..."
            />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowCancelModal(false)}>
            Không, giữ lại
          </Button>
          <Button variant="danger" onClick={handleCancel}>
            Có, hủy lịch
          </Button>
        </Modal.Footer>
      </Modal>
    </main>
  );
};

export default AppointmentUpdate;