// src/pages/Receptionist/CreateMedicalRecord.jsx
import React, { useState } from 'react';
import { Form, Button, Modal } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import instance from '../../axios';
import CustomToast from '../../Components/CustomToast/CustomToast';

const CreateMedicalRecord = () => {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    patientId: '',
    recordNumber: ''
  });

  const [patientName, setPatientName] = useState('');
  const [loading, setLoading] = useState(false);
  const [checking, setChecking] = useState(false);
  const [toast, setToast] = useState(null);
  const [showCreatePatientModal, setShowCreatePatientModal] = useState(false);

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
      return false;
    }
  };

  const submitMedicalRecord = async () => {
    const currentUser = JSON.parse(localStorage.getItem('user') || '{}');

    const payload = {
      patientId: formData.patientId.trim(),
      patientName: patientName,
      recordNumber: formData.recordNumber.trim(),
      issueDate: new Date().toISOString().split('T')[0],
      status: "Active",
      notes: "Cấp lần đầu",
      createdBy: currentUser?.id || currentUser?.staffId || 1
    };

    await instance.post('/MedicalRecord', payload);
    setToast({ type: 'success', message: `Tạo hồ sơ thành công: ${formData.recordNumber.trim()}` });

    // Reset form
    setFormData({ patientId: '', recordNumber: '' });
    setPatientName('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const id = formData.patientId.trim();
    if (!id) {
      setToast({ type: 'error', message: 'Vui lòng nhập mã bệnh nhân!' });
      return;
    }
    if (!formData.recordNumber.trim()) {
      setToast({ type: 'error', message: 'Vui lòng nhập số hồ sơ!' });
      return;
    }
    if (isNaN(id)) {
      setToast({ type: 'error', message: 'Mã bệnh nhân phải là số!' });
      return;
    }

    setLoading(true);
    setChecking(true);

    try {
      const exists = await checkPatientExists(id);

      if (!exists) {
        setShowCreatePatientModal(true);
      } else {
        await submitMedicalRecord();
      }
    } catch (err) {
      setToast({ type: 'error', message: 'Lỗi hệ thống. Vui lòng thử lại!' });
    } finally {
      setChecking(false);
      setLoading(false);
    }
  };

  const handleGoToCreatePatient = () => {
    setShowCreatePatientModal(false);
    navigate('/receptionist/create-patient');
  };

  return (
    <main className="main-content flex-grow-1 p-4">
      {toast && <CustomToast type={toast.type} message={toast.message} onClose={() => setToast(null)} />}

      <div className="bg-white rounded shadow-sm p-4" style={{ maxWidth: '700px', margin: '0 auto' }}>
        <h3 className="mb-4 text-primary fw-bold">Tạo Hồ Sơ Bệnh Án Mới</h3>

        <Form onSubmit={handleSubmit} noValidate>
          {/* MÃ BỆNH NHÂN */}
          <Form.Group className="mb-3">
            <Form.Label>Mã Bệnh Nhân *</Form.Label>
            <Form.Control
              type="text"
              inputMode="numeric"
              value={formData.patientId}
              onChange={(e) => {
                const val = e.target.value;
                if (val === '' || /^\d+$/.test(val)) {
                  setFormData(prev => ({ ...prev, patientId: val }));
                }
              }}
              placeholder="VD: 18, 19, 20..."
              disabled={loading || checking}
            />
            {checking && <small className="text-muted d-block mt-1">Đang kiểm tra...</small>}
            {patientName && !checking && (
              <small className="text-success fw-bold d-block mt-1">
                Đã tìm thấy: <strong>{patientName}</strong>
              </small>
            )}
          </Form.Group>

          {/* SỐ HỒ SƠ */}
          <Form.Group className="mb-4">
            <Form.Label>Số Hồ Sơ Bệnh Án *</Form.Label>
            <Form.Control
              type="text"
              value={formData.recordNumber}
              onChange={(e) => setFormData(prev => ({ ...prev, recordNumber: e.target.value }))}
              placeholder="VD: SKB-111, HS2025-001"
              disabled={loading || checking}
            />
            <Form.Text className="text-muted">
              Nhập số hồ sơ theo quy định phòng khám
            </Form.Text>
          </Form.Group>

          <div className="bg-light p-3 rounded mb-4 small">
            <div><strong>Trạng thái:</strong> Active</div>
            <div><strong>Ghi chú:</strong> Cấp lần đầu</div>
            <div><strong>Ngày cấp:</strong> {new Date().toLocaleDateString('vi-VN')}</div>
          </div>

          <div className="d-flex gap-3">
            <Button
              type="submit"
              variant="primary"
              size="lg"
              className="flex-fill rounded-pill"
              disabled={loading || checking || !formData.patientId || !formData.recordNumber}
            >
              {checking ? 'Đang kiểm tra...' : loading ? 'Đang tạo...' : 'Tạo Hồ Sơ'}
            </Button>

            <Button
              type="button"
              variant="outline-secondary"
              size="lg"
              className="flex-fill rounded-pill"
              onClick={() => {
                setFormData({ patientId: '', recordNumber: '' });
                setPatientName('');
              }}
              disabled={loading || checking}
            >
              Làm mới
            </Button>
          </div>
        </Form>
      </div>

      {/* MODAL: Bệnh nhân chưa tồn tại */}
      <Modal show={showCreatePatientModal} onHide={() => setShowCreatePatientModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Bệnh Nhân Chưa Tồn Tại</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="mb-3">
            Không tìm thấy bệnh nhân với mã <strong>{formData.patientId}</strong>.
          </p>
          <p>Hệ thống sẽ <strong>tự động cấp mã mới</strong> khi tạo bệnh nhân.</p>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowCreatePatientModal(false)}>
            Hủy
          </Button>
          <Button variant="primary" onClick={handleGoToCreatePatient}>
            Tạo Bệnh Nhân Mới
          </Button>
        </Modal.Footer>
      </Modal>
    </main>
  );
};

export default CreateMedicalRecord;