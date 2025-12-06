import React, { useState, useEffect, useCallback, memo } from 'react';
import { Clock, Stethoscope, Printer, Plus, Trash2, User, CheckCircle, PlayCircle } from 'lucide-react';
import Loading from '../../Components/Loading/Loading';
import CustomToast from '../../Components/CustomToast/CustomToast';
import ConfirmModal from '../../Components/CustomToast/DeleteConfirmModal';
import instance from '../../axios';
import queueConnection from '../../signalr/queueHub';
import Select from "react-select";

// Helper functions
const formatTime = (timeString) => {
  if (!timeString) return 'N/A';
  return timeString.substring(0, 5);
};

const formatDate = (dateString) => {
  if (!dateString) return 'N/A';
  return new Date(dateString).toLocaleDateString('vi-VN');
};

// Queue Patient Item Component
const QueuePatientItem = memo(({ patient, isSelected, onSelect, onPrint }) => {
  const getStatusConfig = (status) => {
    switch (status) {
      case 'Completed':
        return { variant: 'success', icon: CheckCircle, text: 'Hoàn thành' };
      case 'InProgress':
        return { variant: 'info', icon: PlayCircle, text: 'Đang khám' };
      case 'Waiting':
      default:
        return { variant: 'warning', icon: Clock, text: 'Đang chờ' };
    }
  };

  const statusConfig = getStatusConfig(patient.status);
  const StatusIcon = statusConfig.icon;

  return (
    <div
      className={`card mb-2 border-0 shadow-sm queue-item ${isSelected ? 'border-primary' : ''} ${patient.status === 'Completed' ? 'opacity-75' : 'hover-shadow'
        }`}
      style={{
        cursor: patient.status !== 'Completed' ? 'pointer' : 'default',
        borderLeft: isSelected ? '4px solid #0d6efd' : '4px solid transparent'
      }}
      onClick={() => patient.status !== 'Completed' && onSelect(patient)}
    >
      <div className="card-body py-3">
        <div className="row align-items-center">
          <div className="col-8">
            <div className="d-flex align-items-center mb-2">
              <span className="badge bg-primary me-2 fs-6">#{patient.queueNumber}</span>
              <h6 className="mb-0 fw-bold text-dark">{patient.patientName}</h6>
            </div>
            <div className="d-flex align-items-center text-muted">
              <Clock size={14} className="me-1" />
              <small>Giờ vào: {formatTime(patient.queueTime)}</small>
            </div>
          </div>
          <div className="col-4 text-end">
            <div className="d-flex flex-column align-items-end gap-2">
              <span className={`badge bg-${statusConfig.variant} text-white`}>
                <StatusIcon size={12} className="me-1" />
                {statusConfig.text}
              </span>
              {patient.status === "Completed" && (
                <button
                  className="btn btn-outline-primary btn-sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    onPrint(patient);
                  }}
                  title="In đơn thuốc"
                >
                  <Printer size={14} />
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
});

// Examination Form Component
const ExaminationForm = memo(({
  patient,
  examinationData,
  allServices,
  allMedicines,
  onClose,
  onDataChange,
  onSubmit,
  isProcessing
}) => {
  const medicineOptions = allMedicines.map(m => ({
    value: m.medicineId,
    label: `${m.medicineName} (${m.medicineType}) - Tồn kho: ${m.stockQuantity}`
  }));

  const handleServiceChange = (serviceId) => {
    const newServiceIds = examinationData.serviceIds.includes(serviceId)
      ? examinationData.serviceIds.filter(id => id !== serviceId)
      : [...examinationData.serviceIds, serviceId];
    onDataChange('serviceIds', newServiceIds);
  };

  const handleAddPrescription = () => {
    const newPrescriptions = [
      ...examinationData.prescriptions,
      { medicineId: '', quantity: 1, dosageInstruction: '' }
    ];
    onDataChange('prescriptions', newPrescriptions);
  };

  const handleRemovePrescription = (index) => {
    const newPrescriptions = examinationData.prescriptions.filter((_, i) => i !== index);
    onDataChange('prescriptions', newPrescriptions);
  };

  const handlePrescriptionChange = (index, field, value) => {
    const newPrescriptions = [...examinationData.prescriptions];
    newPrescriptions[index][field] = field === "medicineId" || field === "quantity"
      ? Number(value)
      : value;
    onDataChange('prescriptions', newPrescriptions);
  };

  return (
    <div className="card border-0 shadow-lg">
      <div className="card-header bg-primary text-white py-3">
        <div className="d-flex justify-content-between align-items-center">
          <div>
            <h5 className="mb-0">
              <Stethoscope className="me-2" size={20} />
              Phiếu Khám Bệnh
            </h5>
            <small>
              Bệnh nhân: <strong>{patient.patientName}</strong> • Số thứ tự: <strong>#{patient.queueNumber}</strong>
            </small>
          </div>
          <button
            className="btn-close btn-close-white"
            onClick={onClose}
            disabled={isProcessing}
          ></button>
        </div>
      </div>

      <div className="card-body">
        {/* Symptoms and Diagnosis */}
        <div className="row g-3 mb-4">
          <div className="col-md-6">
            <label className="form-label fw-semibold text-dark">
              <i className="bi bi-clipboard-pulse me-1"></i>
              Triệu chứng
            </label>
            <textarea
              className="form-control border-2"
              rows="4"
              placeholder="Nhập triệu chứng của bệnh nhân..."
              value={examinationData.symptoms}
              onChange={(e) => onDataChange('symptoms', e.target.value)}
              disabled={isProcessing}
              style={{ borderColor: '#e9ecef' }}
            />
          </div>

          <div className="col-md-6">
            <label className="form-label fw-semibold text-dark">
              <i className="bi bi-clipboard-check me-1"></i>
              Chẩn đoán
            </label>
            <textarea
              className="form-control border-2"
              rows="4"
              placeholder="Nhập kết luận chẩn đoán..."
              value={examinationData.diagnosis}
              onChange={(e) => onDataChange('diagnosis', e.target.value)}
              disabled={isProcessing}
              style={{ borderColor: '#e9ecef' }}
            />
          </div>
        </div>

        {/* Services Section */}
        <div className="mb-4">
          <label className="form-label fw-semibold text-dark mb-3">
            <i className="bi bi-heart-pulse me-1"></i>
            Dịch vụ chỉ định
          </label>
          <div className="row g-2">
            {allServices.map(service => (
              <div key={service.serviceId} className="col-md-6 col-lg-4">
                  <input
                    className="form-check-input"
                    type="checkbox"
                    id={`service-${service.serviceId}`}
                    checked={examinationData.serviceIds.includes(service.serviceId)}
                    onChange={() => handleServiceChange(service.serviceId)}
                    disabled={isProcessing}
                  />
                  <label className="form-check-label card-body py-2" htmlFor={`service-${service.serviceId}`}>
                    <div className="fw-semibold">{service.serviceName}</div>
                    <small className="text-muted">{service.serviceType}</small>
                  </label>
              </div>
            ))}
          </div>
        </div>

        {/* Prescriptions Section */}
        <div className="mb-4">
          <div className="d-flex justify-content-between align-items-center mb-3">
            <label className="form-label fw-semibold text-dark mb-0">
              <i className="bi bi-capsule me-1"></i>
              Đơn thuốc
            </label>
            <button
              type="button"
              className="btn btn-success btn-sm"
              onClick={handleAddPrescription}
              disabled={isProcessing}
            >
              <Plus size={14} className="me-1" />
              Thêm thuốc
            </button>
          </div>

          {examinationData.prescriptions.length === 0 ? (
            <div className="text-center py-4 border rounded bg-light">
              <i className="bi bi-capsule text-muted fs-1 d-block mb-2"></i>
              <p className="text-muted mb-0">Chưa có thuốc nào được kê đơn</p>
            </div>
          ) : (
            examinationData.prescriptions.map((row, index) => (
              <div key={index} className="card border mb-2 prescription-item">
                <div className="card-body">
                  <div className="row g-3 align-items-center">
                    <div className="col-md-5">
                      <label className="form-label small fw-semibold">Tên thuốc</label>
                      <Select
                        options={medicineOptions}
                        value={medicineOptions.find(opt => opt.value === row.medicineId)}
                        onChange={(selected) => handlePrescriptionChange(index, "medicineId", selected?.value || '')}
                        placeholder="Chọn thuốc..."
                        isSearchable={true}
                        isDisabled={isProcessing}
                      />
                    </div>
                    <div className="col-md-2">
                      <label className="form-label small fw-semibold">Số lượng</label>
                      <input
                        type="number"
                        className="form-control"
                        value={row.quantity}
                        onChange={(e) => handlePrescriptionChange(index, 'quantity', e.target.value)}
                        min="1"
                        disabled={isProcessing}
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label small fw-semibold">Hướng dẫn sử dụng</label>
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Liều dùng, thời gian..."
                        value={row.dosageInstruction}
                        onChange={(e) => handlePrescriptionChange(index, 'dosageInstruction', e.target.value)}
                        disabled={isProcessing}
                      />
                    </div>
                    <div className="col-md-1">
                      <label className="form-label small text-white">.</label>
                      <button
                        type="button"
                        className="btn btn-outline-danger w-100"
                        onClick={() => handleRemovePrescription(index)}
                        disabled={isProcessing}
                        title="Xóa thuốc"
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>

        {/* Action Buttons */}
        <div className="d-flex gap-3 justify-content-end pt-4 border-top">
         
          <button
            className="btn btn-lg btn-success"
            onClick={() => onSubmit(true)}
            disabled={isProcessing}
          >
            {isProcessing ? (
              <>
                <span className="spinner-border spinner-border-sm me-2" />
                Đang xử lý...
              </>
            ) : (
              <>
                <CheckCircle size={18} className="me-2" />
                Hoàn tất khám
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
});

// Main Doctor Dashboard Component
const DoctorDashboard = () => {
  const [queuePatients, setQueuePatients] = useState([]);
  const [allServices, setAllServices] = useState([]);
  const [allMedicines, setAllMedicines] = useState([]);
  const [selectedPatient, setSelectedPatient] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [roomId, setRoomId] = useState(null);
  const [toast, setToast] = useState(null);
  const [confirm, setConfirm] = useState({ show: false, isComplete: false });
  const [isProcessing, setIsProcessing] = useState(false);

  const [examinationData, setExaminationData] = useState({
    symptoms: '',
    diagnosis: '',
    prescriptions: [],
    serviceIds: [],
  });

  const showToast = (type, message) => setToast({ type, message });

  const fetchRoomId = useCallback(async () => {
    try {
      const res = await instance.get('Doctor/current-room');
      setRoomId(res.roomId);
    } catch {
      setRoomId(null);
    }
  }, []);

  const fetchQueue = useCallback(async () => {
    if (!roomId) return;
    setIsLoading(true);
    try {
      const today = new Date().toLocaleDateString('en-CA');
      const res = await instance.get(`Doctor/my-queue-today`, {
        params: { date: today },
      });
      setQueuePatients(res.data || []);
    } catch (error) {
      showToast('error', 'Lỗi khi tải danh sách hàng chờ');
    } finally {
      setIsLoading(false);
    }
  }, [roomId]);

  const fetchServicesAndMedicines = useCallback(async () => {
    try {
      const [servicesRes, medicinesRes] = await Promise.all([
        instance.get('Doctor/GetAllServicesByDoctorAsync'),
        instance.get('Doctor/GetAllMedicinesByDoctorAsync'),
      ]);
      setAllServices(servicesRes.content?.items || []);
      setAllMedicines(medicinesRes.content?.items || []);
    } catch (error) {
      showToast('error', 'Lỗi khi tải danh sách dịch vụ/thuốc');
    }
  }, []);

  // SignalR Connection
  useEffect(() => {
    if (!roomId) return;

    queueConnection
      .start()
      .then(() => {
        console.log('SignalR Connected');
        return queueConnection.invoke('JoinGroup', `room_${roomId}`);
      })
      .catch((err) => console.error('SignalR Error:', err));

    queueConnection.on('QueueUpdated', () => {
      console.log('Queue cập nhật realtime');
      fetchQueue();
    });

    return () => {
      queueConnection.off('QueueUpdated');
      queueConnection.stop();
    };
  }, [roomId, fetchQueue]);

  // Initial data fetch
  useEffect(() => {
    fetchRoomId();
    fetchServicesAndMedicines();
  }, [fetchRoomId, fetchServicesAndMedicines]);

  useEffect(() => {
    if (roomId) {
      fetchQueue();
    }
  }, [roomId, fetchQueue]);

  const handleSelectPatient = async (patient) => {
    if (patient.status === 'Waiting') {
      try {
        setIsLoading(true);
        await instance.put(`Queue/start/${patient.queueId}`);
        showToast('success', `Đã bắt đầu khám cho ${patient.patientName}`);
        fetchQueue();
      } catch (error) {
        showToast('error', 'Không thể bắt đầu phiên khám');
        return;
      } finally {
        setIsLoading(false);
      }
    }
    setSelectedPatient(patient);
    setExaminationData({
      symptoms: '',
      diagnosis: '',
      prescriptions: [],
      serviceIds: [],
    });
  };

  const handleExaminationDataChange = (field, value) => {
    setExaminationData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmitExamination = (isComplete) => {
    setConfirm({ show: true, isComplete });
  };

  const processExaminationSubmit = async () => {
    if (!selectedPatient) return;

    setIsProcessing(true);
    try {
      const payload = {
        queueId: selectedPatient.queueId,
        symptoms: examinationData.symptoms,
        diagnosis: examinationData.diagnosis,
        serviceIds: examinationData.serviceIds,
        prescriptions: examinationData.prescriptions,
        isComplete: confirm.isComplete
      };

      await instance.post("Doctor/SubmitExamination", payload);

      showToast(
        "success",
        confirm.isComplete ? "Hoàn tất khám thành công!" : "Đã lưu tạm hồ sơ"
      );

      fetchQueue();
      setSelectedPatient(null);
    } catch (error) {
      showToast("error", "Không thể lưu thông tin khám bệnh");
    } finally {
      setIsProcessing(false);
      setConfirm({ show: false, isComplete: false });
    }
  };

  
  const handlePrintPrescription = async (patient) => {
    try {
      const res = await instance.get(
        `Prescription/GetPrescriptionDetailsAsync/${patient.appoinmentId}`,
      );
      const data = res.content;

      const currentDate = new Date();
      const formattedDate = currentDate.toLocaleDateString("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      });

      const printWindow = window.open("", "_blank");
      printWindow.document.write(`
      <!DOCTYPE html>
      <html lang="vi">
      <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>ĐƠN THUỐC - PHÒNG KHÁM VITACARE</title>
          
          <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet">
          
          <style>
              body {
                  font-family: system-ui, -apple-system, "Segoe UI", Roboto, Arial, sans-serif;
                  background: white;
                  color: black;
                  padding: 10px;
                  line-height: 1.3;
                  font-size: 12px;
                  margin: 0;
                  position: relative;
              }
              
              /* Watermark cho tất cả các trang khi in */
              @media print {
                  @page {
                      size: A5;
                      margin: 10mm;
                  }
                  
                  body::before {
                      content: "";
                      position: absolute;
                      top: 0;
                      left: 0;
                      right: 0;
                      bottom: 0;
                      background-image: url('/logo.png');
                      background-position: center;
                      background-repeat: no-repeat;
                      background-size: 300px auto;
                      opacity: 0.1;
                      z-index: -1;
                      pointer-events: none;
                  }
                  
                  body {
                      padding: 0 !important;
                      background: white !important;
                      font-size: 11px !important;
                      position: relative;
                  }
                  
                  .prescription-container {
                      width: 100% !important;
                      min-height: auto !important;
                      margin: 0 !important;
                      padding: 0;
                      box-shadow: none !important;
                      position: relative;
                      z-index: 1;
                      background: transparent !important;
                  }
                  
                  /* Đảm bảo tất cả nội dung đều có nền trong suốt */
                  .header, .patient-info, .prescription-details, 
                  .medicines-section, .instructions, .footer {
                      background: transparent !important;
                  }
                  
                  .bg-light {
                      background-color: rgba(248, 249, 250, 0.8) !important;
                  }
                  
                  /* Ẩn watermark trên màn hình khi in */
                  .watermark-screen {
                      display: none;
                  }
              }
              
              /* Watermark hiển thị trên màn hình */
              @media screen {
                  .watermark-screen {
                      position: fixed;
                      top: 50%;
                      left: 50%;
                      transform: translate(-50%, -50%) rotate(-30deg);
                      opacity: 0.1;
                      z-index: 1;
                      pointer-events: none;
                  }
                  
                  .watermark-logo {
                      width: 300px;
                      height: auto;
                      filter: grayscale(100%) brightness(1.2);
                  }
              }
              
              .prescription-container {
                  width: 148mm;
                  min-height: 210mm;
                  margin: 0 auto;
                  background: white;
                  position: relative;
                  z-index: 2;
                  padding: 10mm;
              }
              
              .logo {
                  height: 40px;
                  width: auto;
              }

              .info-label {
                  font-weight: 600;
                  min-width: 90px;
                  color: #333;
                  font-size: 11px;
              }
              .info-value {
                  font-weight: 500;
                  flex: 1;
                  font-size: 11px;
              }
              .detail-label {
                  font-weight: 600;
                  min-width: 80px;
                  font-size: 10px;
              }
              .detail-value {
                  flex: 1;
                  font-size: 10px;
              }

              .instructions-list li {
                  margin-bottom: 3px;
                  position: relative;
                  font-size: 9px;
              }
              .instructions-list li:before {
                  content: "•";
                  position: absolute;
                  left: -15px;
                  font-weight: bold;
              }
              
              /* Đảm bảo nội dung hiển thị rõ */
              .content-wrapper {
                  position: relative;
                  z-index: 2;
              }
              
              /* Tăng độ tương phản cho các phần quan trọng */
              .medicines-table {
                  background-color: rgba(255, 255, 255, 0.9) !important;
              }
              
              .table-light {
                  background-color: rgba(248, 249, 250, 0.9) !important;
              }
          </style>
      </head>
      <body>
          <!-- Watermark cho màn hình -->
          <div class="watermark-screen">
              <img src="/logo.png" alt="VitaCare Watermark" class="watermark-logo">
          </div>
          
          <div class="prescription-container">
              <div class="content-wrapper">
                  <div class="header text-center border-bottom pb-3 mb-3">
                      <div class="logo-container mb-2">
                          <img src="/logo.png" alt="VitaCare Logo" class="logo">
                      </div>
                      <div class="clinic-name h5 fw-bold text-uppercase">Phòng Khám Đa Khoa VitaCare</div>
                      <div class="clinic-address small">123 Đường Sức Khỏe, Quận 1, TP.HCM</div>
                      <div class="clinic-phone small">ĐT: 028 1234 5678</div>
                      <div class="prescription-title h4 fw-bolder text-uppercase mt-2">Đơn Thuốc</div>
                  </div>
                  
                  <div class="date-info text-end small text-muted mb-2">
                      Ngày: ${formattedDate} | Mã đơn: DT-${Date.now().toString().slice(-6)}
                  </div>
                  
                  <div class="patient-info mb-3">
                      <div class="row">
                          <div class="col-6">
                              <h6 class="section-title border-bottom pb-1 mb-2 text-uppercase fw-bold fs-6">Bệnh nhân</h6>
                              <div class="d-flex mb-1">
                                  <span class="info-label">Họ tên:</span>
                                  <span class="info-value">${data.patientName}</span>
                              </div>
                              <div class="d-flex mb-1">
                                  <span class="info-label">Mã HS:</span>
                                  <span class="info-value">HS-${patient.appoinmentId.toString().padStart(6, "0")}</span>
                              </div>
                          </div>
                          
                          <div class="col-6">
                              <h6 class="section-title border-bottom pb-1 mb-2 text-uppercase fw-bold fs-6">Bác sĩ</h6>
                              <div class="d-flex mb-1">
                                  <span class="info-label">Bác sĩ:</span>
                                  <span class="info-value">${data.doctorName}</span>
                              </div>
                              <div class="d-flex mb-1">
                                  <span class="info-label">Chuyên khoa:</span>
                                  <span class="info-value">Nội tổng quát</span>
                              </div>
                          </div>
                      </div>
                  </div>
                  
                  <div class="prescription-details bg-light p-2 border rounded my-3">
                      <h6 class="section-title border-bottom pb-1 mb-2 text-uppercase fw-bold fs-6">Chẩn đoán</h6>
                      <div class="d-flex mb-1">
                          <span class="detail-label">Triệu chứng:</span>
                          <span class="detail-value">${data.symptoms || "Không có thông tin"}</span>
                      </div>
                      <div class="d-flex">
                          <span class="detail-label">Chẩn đoán:</span>
                          <span class="detail-value">${data.diagnosis || "Không có thông tin"}</span>
                      </div>
                  </div>
                  
                  <div class="medicines-section my-3">
                      <h6 class="section-title border-bottom pb-1 mb-2 text-uppercase fw-bold fs-6">Đơn thuốc điều trị</h6>
                      
                      <table class="medicines-table table table-bordered table-sm" style="font-size: 10px;">
                          <thead class="table-light text-uppercase">
                              <tr>
                                  <th style="width: 8%">STT</th>
                                  <th style="width: 42%">Tên thuốc</th>
                                  <th style="width: 15%">SL</th>
                                  <th style="width: 35%">Hướng dẫn sử dụng</th>
                              </tr>
                          </thead>
                          <tbody>
                              ${data.medicines && data.medicines.length > 0
          ? data.medicines
            .map(
              (m, index) => `
                                      <tr>
                                          <td class="text-center">${index + 1}</td>
                                          <td>
                                              <div class="medicine-name fw-bold">${m.name || "Không xác định"}</div>
                                          </td>
                                          <td class="medicine-quantity text-center fw-bold">${m.quantity || "1"}</td>
                                          <td class="medicine-usage">${m.usage || "Theo chỉ dẫn của bác sĩ"}</td>
                                      </tr>
                                  `,
            )
            .join("")
          : `
                                      <tr>
                                          <td colspan="4" class="text-center p-3 text-muted">
                                              Không có thuốc nào được kê đơn
                                          </td>
                                      </tr>
                                  `
        }
                          </tbody>
                      </table>
                  </div>
                  
                  <div class="instructions bg-light p-2 border rounded my-3">
                      <h6 class="instructions-title fw-bold text-uppercase fs-6">HƯỚNG DẪN SỬ DỤNG</h6>
                      <ul class="instructions-list list-unstyled ps-3">
                          <li>Uống thuốc đúng liều lượng và thời gian</li>
                          <li>Không tự ý ngưng thuốc</li>
                          <li>Bảo quản thuốc nơi khô ráo</li>
                          <li>Thông báo ngay nếu có tác dụng phụ</li>
                          <li>Tái khám đúng hẹn</li>
                      </ul>
                  </div>
                  
                  <div class="footer border-top pt-3 mt-4">
                      <div class="row">
                          <div class="col-6 text-center">
                              <div class="signature-space border-bottom mb-2" style="height: 60px;"></div>
                              <div class="signature-name fw-bold small text-uppercase">BÁC SĨ ĐIỀU TRỊ</div>
                              <div class="signature-title fst-italic small text-muted">(Ký, ghi rõ họ tên và đóng mộc)</div>
                          </div>
                          <div class="col-6 text-center">
                              <div class="signature-space border-bottom mb-2" style="height: 60px;"></div>
                              <div class="signature-name fw-bold small text-uppercase">BỆNH NHÂN/Người nhà</div>
                              <div class="signature-title fst-italic small text-muted">(Ký và ghi rõ họ tên)</div>
                          </div>
                      </div>
                      
                      <div class="footer-info text-center text-muted small mt-3">
                          <div class="footer-links mb-1">
                              <span class="footer-link mx-2">Hotline: 028 1234 5678</span>
                              <span class="footer-link mx-2">www.vitacare.com</span>
                          </div>
                          <div>
                              Đơn thuốc có hiệu lực trong 30 ngày
                          </div>
                      </div>
                  </div>
              </div>
          </div>
          
          <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
          
          <script>
              setTimeout(() => {
                  window.print();
              }, 500);
          </script>
      </body>
      </html>
    `);
      printWindow.document.close();
    } catch (error) {
      console.error("Error printing prescription:", error);
      showToast("error", "Không thể tải đơn thuốc");
    }
  };

  const stats = {
    total: queuePatients.length,
    waiting: queuePatients.filter(p => p.status === 'Waiting').length,
    inProgress: queuePatients.filter(p => p.status === 'InProgress').length,
    completed: queuePatients.filter(p => p.status === 'Completed').length
  };

  return (
    <div className="container-fluid py-4 bg-light min-vh-100">
      {toast && (
        <CustomToast
          type={toast.type}
          message={toast.message}
          onClose={() => setToast(null)}
        />
      )}

      {isProcessing && <Loading isLoading={true} />}

      {/* Header */}
      <div className="row mb-4">
        <div className="col-12">
          <div className="card border-0 shadow-sm">
            <div className="card-body py-4">
              <div className="row align-items-center">
                <div className="col-md-6">
                  <div className="d-flex align-items-center">
                    <div className="bg-primary rounded-circle p-3 me-3">
                      <User size={24} className="text-white" />
                    </div>
                    <div>
                      <h1 className="h3 fw-bold text-dark mb-1">Bác Sĩ Khám Bệnh</h1>
                      <p className="text-muted mb-0">Quản lý hàng chờ và khám bệnh</p>
                    </div>
                  </div>
                </div>
                <div className="col-md-6">
                  <div className="row text-center">
                    <div className="col-3">
                      <div className="border-end">
                        <div className="h4 fw-bold text-primary">{stats.total}</div>
                        <small className="text-muted">Tổng số</small>
                      </div>
                    </div>
                    <div className="col-3">
                      <div className="border-end">
                        <div className="h4 fw-bold text-warning">{stats.waiting}</div>
                        <small className="text-muted">Đang chờ</small>
                      </div>
                    </div>
                    <div className="col-3">
                      <div className="border-end">
                        <div className="h4 fw-bold text-info">{stats.inProgress}</div>
                        <small className="text-muted">Đang khám</small>
                      </div>
                    </div>
                    <div className="col-3">
                      <div>
                        <div className="h4 fw-bold text-success">{stats.completed}</div>
                        <small className="text-muted">Hoàn thành</small>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="row g-4">
        {/* Queue Panel */}
        <div className="col-lg-4">
          <div className="card border-0 shadow-sm h-100">
            <div className="card-header bg-white border-bottom py-3">
              <div className="d-flex justify-content-between align-items-center">
                <h5 className="mb-0 fw-bold text-dark">
                  <Clock className="me-2" size={20} />
                  Hàng Chờ Khám
                </h5>
                <span className="badge bg-primary">{stats.total}</span>
              </div>
              <small className="text-muted">{formatDate(new Date())} • Phòng {roomId || 'N/A'}</small>
            </div>
            <div className="card-body p-3">
              {isLoading && queuePatients.length === 0 ? (
                <div className="text-center py-5">
                  <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </div>
                  <p className="text-muted mt-2">Đang tải danh sách...</p>
                </div>
              ) : queuePatients.length > 0 ? (
                <div className="queue-list" style={{ maxHeight: '65vh', overflowY: 'auto' }}>
                  {queuePatients.map(patient => (
                    <QueuePatientItem
                      key={patient.queueId}
                      patient={patient}
                      isSelected={selectedPatient?.queueId === patient.queueId}
                      onSelect={handleSelectPatient}
                      onPrint={handlePrintPrescription}
                    />
                  ))}
                </div>
              ) : (
                <div className="text-center py-5 text-muted">
                  <Clock size={48} className="mb-3 opacity-50" />
                  <h6>Không có bệnh nhân</h6>
                  <p className="mb-0">Hàng chờ trống</p>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Examination Panel */}
        <div className="col-lg-8">
          {selectedPatient ? (
            <ExaminationForm
              patient={selectedPatient}
              examinationData={examinationData}
              allServices={allServices}
              allMedicines={allMedicines}
              onClose={() => setSelectedPatient(null)}
              onDataChange={handleExaminationDataChange}
              onSubmit={handleSubmitExamination}
              isProcessing={isProcessing}
            />
          ) : (
            <div className="card border-0 shadow-sm h-100">
              <div className="card-body d-flex flex-column justify-content-center align-items-center text-center py-5">
                <div className="bg-light rounded-circle p-4 mb-3">
                  <Stethoscope size={48} className="text-muted" />
                </div>
                <h4 className="text-muted mb-3">Chưa chọn bệnh nhân</h4>
                <p className="text-muted mb-0 text-center">
                  Vui lòng chọn một bệnh nhân từ danh sách hàng chờ <br /> để bắt đầu quá trình khám bệnh
                </p>
              </div>
            </div>
          )}
        </div>
      </div>

      <ConfirmModal
        isOpen={confirm.show}
        title={confirm.isComplete ? 'Xác nhận hoàn tất khám' : 'Xác nhận lưu tạm'}
        message={
          confirm.isComplete
            ? 'Bạn có chắc muốn hoàn tất và lưu vĩnh viễn hồ sơ khám bệnh này?'
            : 'Bạn có muốn lưu tạm thời thông tin đã nhập?'
        }
        onConfirm={processExaminationSubmit}
        onCancel={() => setConfirm({ show: false, isComplete: false })}
        buttonLabel="Xác nhận"
        isProcessing={isProcessing}
       
      />

      <style jsx>{`
        .hover-shadow:hover {
          box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15) !important;
        }
        
        .queue-item {
          transition: all 0.2s ease-in-out;
        }
        
        .prescription-item {
          background-color: #f8f9fa;
        }
        
        .queue-list::-webkit-scrollbar {
          width: 6px;
        }
        
        .queue-list::-webkit-scrollbar-track {
          background: #f1f1f1;
          border-radius: 3px;
        }
        
        .queue-list::-webkit-scrollbar-thumb {
          background: #c1c1c1;
          border-radius: 3px;
        }
        
        .queue-list::-webkit-scrollbar-thumb:hover {
          background: #a8a8a8;
        }
      `}</style>
    </div>
  );
};

export default DoctorDashboard;