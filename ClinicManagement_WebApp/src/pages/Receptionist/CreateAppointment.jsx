import React, { useState, useEffect } from 'react';
import { Form, Button, Modal } from 'react-bootstrap';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';
import { vi } from 'date-fns/locale';
import { useNavigate } from 'react-router-dom';
import instance from '../../axios';
import CustomToast from '../../Components/CustomToast/CustomToast';

const CreateAppointment = () => {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    patientId: '',
    staffId: '',
    appointmentTime: null,
    notes: '',
    roomId: '',
    // scheduleId: '',
    recordId: ''
  });

  // === DANH SÁCH DỮ LIỆU ===
  const [staffList, setStaffList] = useState([]);
  const [roomList, setRoomList] = useState([]);
  // const [scheduleList, setScheduleList] = useState([]);
  const [recordList, setRecordList] = useState([]);

  // === TRẠNG THÁI GIAO DIỆN ===
  const [patientName, setPatientName] = useState('');
  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState(null);
  const [isCheckingPatient, setIsCheckingPatient] = useState(false);
  const [showCreatePatientModal, setShowCreatePatientModal] = useState(false);

  useEffect(() => {
    const fetchStaff = async () => {
      try {
        const res = await instance.get('Admin/GetAllMedicalStaffAsync');
        const data = res.data?.content || res.data || res.content || [];
        setStaffList(data);
      } catch (err) {
        setToast({ type: 'error', message: 'Không tải được danh sách bác sĩ.' });
      }
    };
    fetchStaff();
  }, []);

  useEffect(() => {
    const fetchOthers = async () => {
      try {
        const [roomRes, recordRes] = await Promise.all([
          instance.get('room').catch(() => ({ data: [] })),
          // instance.get('Schedule/GetAllSchedulesAsync'),
          instance.get('MedicalRecord').catch(() => ({ data: [] }))
        ]);

        setRoomList(roomRes.data?.content || roomRes.data || []);
        setRecordList(recordRes.data?.content || recordRes.data || []);
      } catch (err) {
        console.warn('Lỗi load dữ liệu phụ', err);
      }
    };
    fetchOthers();
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleDateChange = (date) => {
    setFormData(prev => ({ ...prev, appointmentTime: date }));
  };


  const checkPatientExists = async (patientIdStr) => {
    const id = patientIdStr.trim();
    if (!id || isNaN(id)) return false;

    try {
      const res = await instance.get('Patient/patientId', {
        params: { id: Number(id) }
      });

      const data = res.data?.data || res.data;
      if (data && (data.patientId !== undefined || data.id !== undefined)) {
        const name = data.patientName || data.fullName || 'Không rõ tên';
        setPatientName(name);
        return true;
      }
      return false;
    } catch (err) {
      console.error('Lỗi kiểm tra bệnh nhân:', err.response || err);
      setPatientName('');
      return false;
    }
  };


  const submitAppointment = async () => {
    const selectedDate = formData.appointmentTime;
    const appointmentDate = selectedDate.toISOString().split('T')[0];
    const hours = selectedDate.getHours().toString().padStart(2, '0');
    const minutes = selectedDate.getMinutes().toString().padStart(2, '0');
    const appointmentTimeStr = `${hours}:${minutes}:00`;

    const payload = {
      patientId: formData.patientId.trim(),
      staffId: Number(formData.staffId),
      appointmentDate,
      appointmentTime: appointmentTimeStr,
      notes: formData.notes?.trim() || '',
      roomId: formData.roomId ? Number(formData.roomId) : null,
      // scheduleId:
      recordId: formData.recordId ? Number(formData.recordId) : null
    };

    const response = await instance.post('Appointment', payload);

    // === TỰ ĐỘNG TẠO QUEUE SAU KHI TẠO LỊCH ===
    let appointmentId = null;
    try {
      const d = response.data;
      if (typeof d === 'number') appointmentId = d;
      else if (d?.appointmentId) appointmentId = d.appointmentId;
      else if (d?.id) appointmentId = d.id;
      else if (d?.content?.appointmentId) appointmentId = d.content.appointmentId;
      else if (d?.content?.id) appointmentId = d.content.id;
      else if (d?.data?.id) appointmentId = d.data.id;
      else if (d?.content && typeof d.content === 'number') appointmentId = d.content;
      else if (d?.data && typeof d.data === 'number') appointmentId = d.data;
    } catch (err) { /* ignore */ }

    if (appointmentId) {
      try {
        await instance.post('/api/Queue/CreateQueueAsync', {
          appointmentId: Number(appointmentId)
        });
      } catch (err) {
        console.warn('Tạo queue thất bại (lịch vẫn được tạo)', err);
      }
    }

    setToast({ type: 'success', message: 'Tạo lịch hẹn thành công!' });
    setFormData({
      patientId: '', staffId: '', appointmentTime: null, notes: '',
      roomId: '', recordId: ''
    });
    setPatientName('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!formData.patientId.trim() || !formData.staffId || !formData.appointmentTime) {
      setToast({ type: 'error', message: 'Vui lòng điền đầy đủ thông tin bắt buộc.' });
      return;
    }
    if (isNaN(formData.patientId.trim())) {
      setToast({ type: 'error', message: 'Mã bệnh nhân phải là số!' });
      return;
    }

    setLoading(true);
    setIsCheckingPatient(true);

    try {
      const exists = await checkPatientExists(formData.patientId);
      if (!exists) {
        setShowCreatePatientModal(true);
      } else {
        await submitAppointment();
      }
    } catch (err) {
      setToast({ type: 'error', message: 'Có lỗi xảy ra!' });
    } finally {
      setIsCheckingPatient(false);
      setLoading(false);
    }
  };

  const handleGoToCreatePatient = () => {
    setShowCreatePatientModal(false);
    navigate(`/create-patient?patientId=${formData.patientId.trim()}`);
  };

  return (
    <main className="main-content flex-grow-1 p-4">
      {toast && <CustomToast type={toast.type} message={toast.message} onClose={() => setToast(null)} />}

      <div className="bg-white rounded shadow-sm p-4" style={{ maxWidth: '800px', margin: '0 auto' }}>
        <h3 className="mb-4 text-primary fw-bold">Tạo Lịch Khám Mới</h3>

        <Form onSubmit={handleSubmit}>
          {/* Mã bệnh nhân */}
          <Form.Group className="mb-3">
            <Form.Label>Mã Bệnh Nhân *</Form.Label>
            <Form.Control
              type="text"
              inputMode="numeric"
              value={formData.patientId}
              onChange={handleChange}
              name="patientId"
              placeholder="VD: 123"
              required
              disabled={loading}
            />
            {isCheckingPatient && <small className="text-muted d-block mt-1">Đang kiểm tra...</small>}
            {patientName && !isCheckingPatient && (
              <small className="text-success fw-bold d-block mt-1">
                Đã tìm thấy: <strong>{patientName}</strong>
              </small>
            )}
          </Form.Group>

          {/* Bác sĩ */}
          <Form.Group className="mb-3">
            <Form.Label>Bác Sĩ Phụ Trách *</Form.Label>
            <Form.Select name="staffId" value={formData.staffId} onChange={handleChange} required disabled={loading}>
              <option value="">Chọn bác sĩ</option>
              {staffList.map(s => (
                <option key={s.staffId} value={s.staffId}>
                  {s.staffName} ({s.staffType || 'Bác sĩ'})
                </option>
              ))}
            </Form.Select>
          </Form.Group>

          {/* Phòng khám */}
          <Form.Group className="mb-3">
            <Form.Label>Phòng Khám</Form.Label>
            <Form.Select name="roomId" value={formData.roomId} onChange={handleChange} disabled={loading}>
              <option value="">Không chọn phòng</option>
              {roomList.map(r => (
                <option key={r.roomId || r.id} value={r.roomId || r.id}>
                  {r.roomName || r.name}
                </option>
              ))}
            </Form.Select>
          </Form.Group>

          {/* Hồ sơ bệnh án */}
          <Form.Group className="mb-3">
            <Form.Label>Hồ Sơ Bệnh Án</Form.Label>
            <Form.Select name="recordId" value={formData.recordId} onChange={handleChange} disabled={loading}>
              <option value="">Không chọn hồ sơ</option>
              {recordList.map(r => (
                <option key={r.recordId || r.id} value={r.recordId || r.id}>
                  {r.recordNumber}
                </option>
              ))}
            </Form.Select>
          </Form.Group>

          {/* Thời gian khám - lễ tân tự chọn giờ */}
          <Form.Group className="mb-3">
            <Form.Label>Thời Gian Khám *</Form.Label>
            <DatePicker
              selected={formData.appointmentTime}
              onChange={handleDateChange}
              showTimeSelect
              timeIntervals={30}
              timeFormat="HH:mm"
              dateFormat="dd/MM/yyyy - HH:mm"
              locale={vi}
              placeholderText="Chọn ngày giờ khám"
              className="form-control"
              minDate={new Date()}
              required
              disabled={loading}
            />
          </Form.Group>

          {/* Ghi chú */}
          <Form.Group className="mb-4">
            <Form.Label>Ghi Chú</Form.Label>
            <Form.Control as="textarea" rows={3} name="notes" value={formData.notes} onChange={handleChange} disabled={loading} />
          </Form.Group>

          {/* Nút hành động */}
          <div className="d-flex gap-3">
            <Button
              type="submit"
              variant="success"
              size="lg"
              className="flex-fill rounded-pill"
              disabled={loading || isCheckingPatient}
            >
              {isCheckingPatient ? 'Đang kiểm tra...' : loading ? 'Đang tạo...' : 'Tạo Lịch Khám'}
            </Button>
            <Button
              type="button"
              variant="outline-secondary"
              size="lg"
              className="flex-fill rounded-pill"
              onClick={() => setFormData({
                patientId: '', staffId: '', appointmentTime: null, notes: '',
                roomId: '', recordId: ''
              })}
              disabled={loading}
            >
              Làm mới
            </Button>
          </div>
        </Form>
      </div>

      {/* Modal tạo bệnh nhân mới */}
      <Modal show={showCreatePatientModal} onHide={() => setShowCreatePatientModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Bệnh Nhân Chưa Tồn Tại</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p>Không tìm thấy bệnh nhân với mã <strong>{formData.patientId}</strong>.</p>
          <p>Tạo bệnh nhân mới trước khi đặt lịch?</p>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowCreatePatientModal(false)}>Hủy</Button>
          <Button variant="primary" onClick={handleGoToCreatePatient}>Tạo Bệnh Nhân Mới</Button>
        </Modal.Footer>
      </Modal>
    </main>
  );
};

export default CreateAppointment;