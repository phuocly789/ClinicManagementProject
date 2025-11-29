// src/pages/Patient/CreatePatient.jsx
import React, { useState, useEffect } from 'react';
import { Form, Button, Spinner, Alert } from 'react-bootstrap';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';
import { vi } from 'date-fns/locale';
import { useLocation, useNavigate } from 'react-router-dom';
import instance from '../../axios';
import CustomToast from '../../Components/CustomToast/CustomToast';

const CreatePatient = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    phoneNumber: '',
    dateOfBirth: null,
    gender: 'Nam',
    address: '',
    password: '',
    confirmPassword: '',
    patientId: ''
  });

  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState(null);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const suggestedId = params.get('patientId');
    if (suggestedId) {
      setFormData(prev => ({ ...prev, patientId: suggestedId }));
      setToast({ type: 'info', message: `Gợi ý mã bệnh nhân: ${suggestedId}` });
    }
  }, [location]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const handleDateChange = (date) => {
    setFormData(prev => ({ ...prev, dateOfBirth: date }));
  };

  const validateForm = () => {
    const newErrors = {};

    if (!formData.fullName.trim()) newErrors.fullName = 'Vui lòng nhập họ tên';
    if (!formData.email.includes('@')) newErrors.email = 'Email không hợp lệ';
    if (!formData.phoneNumber.match(/^0[3|5|7|8|9]\d{8}$/)) {
      newErrors.phoneNumber = 'Số điện thoại không hợp lệ (10 số, bắt đầu 03,05,07,08,09)';
    }
    if (!formData.dateOfBirth) newErrors.dateOfBirth = 'Vui lòng chọn ngày sinh';
    if (!formData.address.trim()) newErrors.address = 'Vui lòng nhập địa chỉ';
    if (formData.password.length < 6) newErrors.password = 'Mật khẩu ít nhất 6 ký tự';
    if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Mật khẩu xác nhận không khớp';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;

    setLoading(true);

    const payload = {
      fullName: formData.fullName.trim(),
      email: formData.email.trim(),
      phoneNumber: formData.phoneNumber,
      dateOfBirth: formData.dateOfBirth.toISOString().split('T')[0],
      gender: formData.gender,
      address: formData.address.trim(),
      password: formData.password,
      confirmPassword: formData.confirmPassword
    };

    try {
      const res = await instance.post('Auth/PatientRegister', payload);

      setToast({
        type: 'success',
        message: res.data?.message || 'Đăng ký bệnh nhân thành công!'
      });

      // Nếu có patientId gợi ý → quay lại đặt lịch
      if (formData.patientId) {
        setTimeout(() => {
          navigate('/create-appointment', {
            state: { patientId: formData.patientId }
          });
        }, 1500);
      } else {
        // Reset form
        setFormData({
          fullName: '', email: '', phoneNumber: '', dateOfBirth: null,
          gender: 'Nam', address: '', password: '', confirmPassword: '', patientId: ''
        });
      }

    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data?.title || 'Đăng ký thất bại. Vui lòng thử lại.';
      setToast({ type: 'error', message: msg });
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="main-content flex-grow-1 p-4">
      {toast && <CustomToast type={toast.type} message={toast.message} onClose={() => setToast(null)} />}

      <div className="bg-white rounded shadow-sm p-4" style={{ maxWidth: '750px', margin: '0 auto' }}>
        <h3 className="mb-4 text-primary fw-bold text-center">
          Đăng Ký Bệnh Nhân Mới
        </h3>

        {formData.patientId && (
          <Alert variant="info" className="mb-4">
            Mã bệnh nhân gợi ý: <strong>{formData.patientId}</strong>
            <br /><small>Bạn có thể dùng mã này khi đặt lịch sau khi đăng ký xong.</small>
          </Alert>
        )}

        <Form onSubmit={handleSubmit}>
          <div className="row">
            <div className="col-md-6">
              <Form.Group className="mb-3">
                <Form.Label>Họ và tên *</Form.Label>
                <Form.Control
                  type="text"
                  name="fullName"
                  value={formData.fullName}
                  onChange={handleChange}
                  isInvalid={!!errors.fullName}
                />
                <Form.Control.Feedback type="invalid">{errors.fullName}</Form.Control.Feedback>
              </Form.Group>
            </div>

            <div className="col-md-6">
              <Form.Group className="mb-3">
                <Form.Label>Email *</Form.Label>
                <Form.Control
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  isInvalid={!!errors.email}
                />
                <Form.Control.Feedback type="invalid">{errors.email}</Form.Control.Feedback>
              </Form.Group>
            </div>
          </div>

          <div className="row">
            <div className="col-md-6">
              <Form.Group className="mb-3">
                <Form.Label>Số điện thoại *</Form.Label>
                <Form.Control
                  type="text"
                  name="phoneNumber"
                  value={formData.phoneNumber}
                  onChange={handleChange}
                  placeholder="0901234567"
                  isInvalid={!!errors.phoneNumber}
                />
                <Form.Control.Feedback type="invalid">{errors.phoneNumber}</Form.Control.Feedback>
              </Form.Group>
            </div>

            <div className="col-md-6">
              <Form.Group className="mb-3">
                <Form.Label>Ngày sinh *</Form.Label>
                <DatePicker
                  selected={formData.dateOfBirth}
                  onChange={handleDateChange}
                  dateFormat="dd/MM/yyyy"
                  locale={vi}
                  className={`form-control ${errors.dateOfBirth ? 'is-invalid' : ''}`}
                  placeholderText="Chọn ngày sinh"
                  showYearDropdown
                  scrollableYearDropdown
                  yearDropdownItemNumber={50}
                  maxDate={new Date()}
                />
                {errors.dateOfBirth && <div className="invalid-feedback d-block">{errors.dateOfBirth}</div>}
              </Form.Group>
            </div>
          </div>

          <div className="row">
            <div className="col-md-6">
              <Form.Group className="mb-3">
                <Form.Label>Giới tính</Form.Label>
                <Form.Select name="gender" value={formData.gender} onChange={handleChange}>
                  <option value="Nam">Nam</option>
                  <option value="Nữ">Nữ</option>
                  <option value="Khác">Khác</option>
                </Form.Select>
              </Form.Group>
            </div>

            <div className="col-md-6">
              <Form.Group className="mb-3">
                <Form.Label>Địa chỉ *</Form.Label>
                <Form.Control
                  type="text"
                  name="address"
                  value={formData.address}
                  onChange={handleChange}
                  isInvalid={!!errors.address}
                />
                <Form.Control.Feedback type="invalid">{errors.address}</Form.Control.Feedback>
              </Form.Group>
            </div>
          </div>

          <div className="row">
            <div className="col-md-6">
              <Form.Group className="mb-3">
                <Form.Label>Mật khẩu *</Form.Label>
                <Form.Control
                  type="password"
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  isInvalid={!!errors.password}
                />
                <Form.Control.Feedback type="invalid">{errors.password}</Form.Control.Feedback>
              </Form.Group>
            </div>

            <div className="col-md-6">
              <Form.Group className="mb-3">
                <Form.Label>Xác nhận mật khẩu *</Form.Label>
                <Form.Control
                  type="password"
                  name="confirmPassword"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  isInvalid={!!errors.confirmPassword}
                />
                <Form.Control.Feedback type="invalid">{errors.confirmPassword}</Form.Control.Feedback>
              </Form.Group>
            </div>
          </div>

          <div className="d-flex gap-3 mt-4">
            <Button
              type="submit"
              variant="primary"
              className="flex-fill rounded-pill"
              disabled={loading}
            >
              {loading ? <>Đang đăng ký...</> : 'Đăng Ký Bệnh Nhân'}
            </Button>
            <Button
              type="button"
              variant="outline-secondary"
              className="flex-fill rounded-pill"
              onClick={() => navigate(-1)}
            >
              Quay lại
            </Button>
          </div>
        </Form>
      </div>
    </main>
  );
};

export default CreatePatient;