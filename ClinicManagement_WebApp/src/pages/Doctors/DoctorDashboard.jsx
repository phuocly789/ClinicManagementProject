import React, {
  useState,
  useEffect,
  useCallback,
  memo,
  useRef,
} from 'react';
import {
  Calendar,
  CheckSquare,
  Clock,
  List,
  Stethoscope,
  User,
  X,
  Printer,
  Plus,
  Trash2,
  Search,
} from 'lucide-react';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import Loading from '../../Components/Loading/Loading';
import CustomToast from '../../Components/CustomToast/CustomToast';
import ConfirmDeleteModal from '../../Components/CustomToast/DeleteConfirmModal';
import Pagination from '../../Components/Pagination/Pagination';
import instance from '../../axios';
// import queueConnection from '../signalr/queueConnection';
// import { queueConnection } from '../../signalr/queueConnection';
import '../../App.css';
import queueConnection from '../../signalr/queueHub';
import Select from "react-select";

// --- Helper Functions ---
const formatVND = (value) => {
  if (value === null || value === undefined) return 'N/A';
  return Number(value).toLocaleString('vi-VN', {
    style: 'currency',
    currency: 'VND',
  });
};

const formatDate = (dateString) => {
  if (!dateString) return 'N/A';
  return new Date(dateString).toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });
};

// --- COMPONENT CON 2: Lịch làm việc (FullCalendar) ---
const ScheduleSection = memo(({ events, isLoading }) => (
  <div className="card shadow-sm border-0 table-panel">
    {isLoading ? (
      <Loading isLoading={true} />
    ) : (
      <div className="card-body calendar-panel p-4">
        <FullCalendar
          plugins={[dayGridPlugin, interactionPlugin]}
          initialView="dayGridMonth"
          events={events}
          locale="vi"
          headerToolbar={{
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,dayGridWeek',
          }}
          eventBackgroundColor="#0d6efd"
          eventBorderColor="#0d6efd"
        />
      </div>
    )}
  </div>
));

// --- COMPONENT CON 3: Lịch sử bệnh nhân ---
const HistorySection = memo(() => (
  <div className="card shadow-sm border-0">
    <div className="card-body text-center p-5">
      <List size={48} className="mx-auto text-muted" />
      <h5 className="mt-3 text-muted">
        Chức năng Lịch sử bệnh nhân đang được phát triển.
      </h5>
    </div>
  </div>
));

// --- COMPONENT CHÍNH: DoctorDashboard ---
const DoctorDashboard = () => {
  const [currentSection, setCurrentSection] = useState('today');
  const [queuePatients, setQueuePatients] = useState([]);
  const [scheduleEvents, setScheduleEvents] = useState([]);
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

  const fetchRoomId = useCallback(async () => {
    try {
      const res = await instance.get('Doctor/current-room');
      setRoomId(res.roomId);
    } catch {
      setRoomId(null);
    }
  }, []);

  const showToast = (type, message) => setToast({ type, message });

  

  useEffect(() => {
    fetchRoomId();
  }, [fetchRoomId]);

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
      showToast('error', 'Lỗi khi tải hàng chờ.');
    } finally {
      setIsLoading(false);
    }
  }, [roomId]);
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

  const medicineOptions = allMedicines.map(m => ({
    value: m.medicineId,
    label: m.medicineName,
  }))

  const fetchServicesAndMedicines = useCallback(async () => {
    if (allServices.length > 0 && allMedicines.length > 0) return;
    try {
      const [servicesRes, medicinesRes] = await Promise.all([
        instance.get('Doctor/GetAllServicesByDoctorAsync'),
        instance.get('Doctor/GetAllMedicinesByDoctorAsync'),
      ]);
      setAllServices(servicesRes.content?.items || []);
      setAllMedicines(medicinesRes.content?.items || []);
    } catch (error) {
      showToast('error', 'Lỗi khi tải danh sách dịch vụ/thuốc.');
    }
  }, [allServices.length, allMedicines.length]);

  const fetchSchedule = useCallback(async () => {
    setIsLoading(true);
    try {
      const res = await instance.get('Doctor/GetMySchedule');
      const formattedEvents = (res.content || []).map((item) => ({
        title: item.patientName,
        start: `${item.appointmentDate}T${item.appointmentTime}`,
        extendedProps: item,
      }));
      setScheduleEvents(formattedEvents);
    } catch (error) {
      showToast('error', 'Lỗi khi tải lịch làm việc.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    if (currentSection === 'today') {
      fetchQueue();
      fetchServicesAndMedicines();
    } else if (currentSection === 'schedule') {
      fetchSchedule();
    }
    setSelectedPatient(null);
  }, [
    currentSection,
    fetchQueue,
    fetchSchedule,
    fetchServicesAndMedicines,
  ]);

  const handleSelectPatient = async (patient) => {
    if (patient.status === 'Waiting') {
      try {
        setIsLoading(true);
        await instance.put(`Queue/start/${patient.queueId}`);
        showToast(
          'info',
          `Bắt đầu khám cho bệnh nhân ${patient.patientName}`
        );
        fetchQueue();
      } catch (error) {
        showToast('error', 'Không thể bắt đầu phiên khám.');
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

  const handleConfirmSubmit = (isComplete) => {
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
        isComplete: confirm.isComplete // ✅ Gửi trạng thái hoàn tất/tạm lưu
      };

      await instance.post("Doctor/SubmitExamination", payload);

      showToast(
        "success",
        confirm.isComplete
          ? "Hoàn tất khám thành công!"
          : "Đã lưu tạm hồ sơ."
      );

      fetchQueue();
      setSelectedPatient(null);
    } catch (error) {
      showToast("error", "Không thể lưu khám bệnh.");
    } finally {
      setIsProcessing(false);
      setConfirm({ show: false, isComplete: false });
    }
  };

  const handleServiceChange = (serviceId) => {
    setExaminationData((prev) => {
      const newServiceIds = prev.serviceIds.includes(serviceId)
        ? prev.serviceIds.filter((id) => id !== serviceId)
        : [...prev.serviceIds, serviceId];
      return { ...prev, serviceIds: newServiceIds };
    });
  };

  const handleAddPrescriptionRow = () => {
    setExaminationData((prev) => ({
      ...prev,
      prescriptions: [
        ...prev.prescriptions,
        { medicineId: '', quantity: 1, dosageInstruction: '' },
      ],
    }));
  };

  const handleRemovePrescriptionRow = (index) => {
    setExaminationData((prev) => ({
      ...prev,
      prescriptions: prev.prescriptions.filter((_, i) => i !== index),
    }));
  };

  const handlePrescriptionChange = (index, field, value) => {
    setExaminationData((prev) => {
      const newPrescriptions = [...prev.prescriptions];

      newPrescriptions[index][field] =
        field === "medicineId" || field === "quantity"
          ? Number(value)   // ✅ Ép kiểu số
          : value;

      return { ...prev, prescriptions: newPrescriptions };
    });
  };

  const handlePrintPrescription = async (patient) => {
    try {
      console.log(patient);

      const res = await instance.get(`Prescription/GetPrescriptionDetailsAsync/${patient.appoinmentId}`);
      const data = res.content;

      const printWindow = window.open("", "_blank");
      printWindow.document.write(`
      <html>
<head>
    <title>Đơn thuốc - Phòng Khám VitaCare</title>
    <style>
        body {
            /* Sử dụng màu nền xám nhạt để làm nổi bật tờ đơn thuốc */
            background-color: #f4f7f6;
            font-family: Arial, sans-serif;
            padding: 20px;
            color: #333; /* Màu chữ chính đậm hơn */
        }
        .container {
            /* Tạo một container trắng giống tờ giấy */
            max-width: 800px;
            margin: 20px auto; /* Tự động căn giữa */
            padding: 30px;
            background-color: #ffffff;
            border-radius: 10px; /* Bo góc nhẹ */
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.08); /* Đổ bóng mềm */
            border: 1px solid #eee;
        }

        .header {
            text-align: center;
            padding-bottom: 20px;
            border-bottom: 2px solid #f0f0f0; /* Đường kẻ phân cách */
            margin-bottom: 20px;
        }

        .clinic-name {
            /* Định dạng màu và kích thước cho tên phòng khám */
            color: #005a9c; /* Màu xanh dương chuyên nghiệp */
            font-size: 28px;
            font-weight: bold;
            margin: 0;
        }

        h2 {
            text-align: center;
            color: #111; /* Màu đen cho tiêu đề chính */
            font-size: 24px;
            margin-top: 10px;
            text-transform: uppercase; /* VIẾT HOA */
        }

        .info p {
            font-size: 16px;
            line-height: 1.7; /* Tăng dãn dòng cho dễ đọc */
            margin: 10px 0;
        }

        .info p strong {
            display: inline-block;
            width: 120px; /* Cố định độ rộng của nhãn (Bệnh nhân, Bác sĩ...) */
            color: #555;
        }

        /* --- Định dạng bảng --- */
        table {
            width: 100%;
            border-collapse: collapse; /* Gộp viền */
            margin-top: 25px;
        }

        th, td {
            border: 1px solid #ddd; /* Viền xám nhạt */
            padding: 12px; /* Tăng padding cho thoáng */
            text-align: left;
        }

        th {
            /* Nền xám nhạt cho tiêu đề cột */
            background-color: #f9f9f9;
            font-weight: bold;
            color: #444;
        }

        /* --- Phần ký tên --- */
        .signature {
            margin-top: 60px;
            text-align: right;
            width: 50%;
            margin-left: auto; /* Đẩy sang bên phải */
        }

        .signature p {
            margin: 5px 0;
        }

        .doctor-sign-name {
            font-weight: bold;
            margin-top: 10px;
        }

    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1 class="clinic-name">Phòng Khám VitaCare</h1>
            <h2>ĐƠN THUỐC</h2>
        </div>

        <div class="info">
            <p><strong>Bệnh nhân:</strong> ${data.patientName}</p>
            <p><strong>Bác sĩ:</strong> ${data.doctorName}</p>
            <p><strong>Triệu chứng:</strong> ${data.symptoms}</p>
            <p><strong>Chẩn đoán:</strong> ${data.diagnosis}</p>
        </div>

        <table>
            <thead> <tr>
                    <th>Thuốc</th>
                    <th>Số lượng</th>
                    <th>Hướng dẫn</th>
                </tr>
            </thead>
            <tbody> ${data.medicines.map(m =>
        `<tr><td>${m.name}</td><td>${m.quantity}</td><td>${m.usage}</td></tr>`
      ).join("")}
            </tbody>
        </table>

        <div class="signature">
            <p><i>Ngày..... tháng..... năm.....</i></p>
            <p class="doctor-sign-name">Bác sĩ điều trị</p>
            <br><br><br>
            <p>(Ký và ghi rõ họ tên)</p>
        </div>
    </div>
</body>
</html>
    `);

      printWindow.document.close();
      printWindow.print();

    } catch {
      showToast("error", "Không thể tải đơn thuốc.");
    }
  };


  return (

    <main className="flex-grow-1 p-4 d-flex flex-column gap-4">
      {toast && (
        <CustomToast
          type={toast.type}
          message={toast.message}
          onClose={() => setToast(null)}
        />
      )}
      {isProcessing && (
        <div className="loading-overlay">
          <Loading isLoading={true} />
        </div>
      )}
      <header className="d-flex justify-content-between align-items-center flex-shrink-0">
        <h1 className="h4 mb-0">Bảng Điều Khiển Bác Sĩ</h1>
      </header>

      {isLoading && queuePatients.length === 0 && <Loading isLoading={true} />}

      {currentSection === 'today' && (
        <div className="row g-4">
          {/* Cột hàng chờ */}
          <div className="col-lg-4">
            <div className="card shadow-sm border-0 h-100">
              <div className="card-header fw-bold bg-light">
                Hàng chờ khám ({formatDate(new Date())})
              </div>
              <div
                className="list-group list-group-flush"
                style={{ maxHeight: '75vh', overflowY: 'auto' }}
              >
                {queuePatients.length > 0 ? (
                  queuePatients
                    // .filter((p) => p.status !== 'Completed')
                    .map((p) => (
                      <a
                        href="#"
                        key={p.queueId}
                        onClick={(e) => {
                          e.preventDefault();
                          if (p.status !== "Completed") {
                            handleSelectPatient(p);
                          }
                        }}
                        className={`list-group-item list-group-item-action d-flex justify-content-between align-items-start ${selectedPatient?.queueId === p.queueId ? 'active' : ''
                          }`}
                      >
                        <div className={`ms-2 me-auto ${p.status === "Completed" ? 'disabled opacity-50' : ''}`}>
                          <div className="fw-bold">
                            {p.queueNumber}. {p.patientName}
                          </div>
                          <small>Giờ vào chờ: {p.queueTime}</small>
                        </div>


                        <div className="text-end d-flex flex-column align-items-end">
                          <span
                            className={`badge mb-1 rounded-pill ${p.status === 'Completed'
                              ? 'bg-success'
                              : p.status === 'In Progress'
                                ? 'bg-info'
                                : 'bg-warning'
                              }`}
                          >
                            {p.status}
                          </span>

                          {p.status === "Completed" && (
                            <button
                              className="btn btn-light btn-sm border"
                              title="In đơn thuốc"
                              onClick={() => handlePrintPrescription(p)}
                            >
                              <Printer size={16} />
                            </button>
                          )}
                        </div>

                      </a>
                    ))
                ) : (
                  <div className="text-center p-5 text-muted">
                    Không có bệnh nhân trong hàng chờ.
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Cột form khám bệnh */}
          <div className="col-lg-8">
            {selectedPatient ? (
              <div className="card shadow-sm border-0">
                <div className="card-header bg-light d-flex justify-content-between align-items-center">
                  <span className="fw-bold">
                    Bệnh án: {selectedPatient.patientName} (Số{' '}
                    {selectedPatient.queueNumber})
                  </span>

                  <button
                    className="btn-close"
                    onClick={() => setSelectedPatient(null)}
                  ></button>
                </div>
                <div className="card-body p-4">
                  {/* Chẩn đoán */}
                  <div className="mb-3">
                    <label className="form-label fw-bold">Chẩn đoán</label>
                    <textarea
                      className="form-control mb-2"
                      rows="3"
                      placeholder="Triệu chứng của bệnh nhân..."
                      value={examinationData.symptoms}
                      onChange={(e) =>
                        setExaminationData({
                          ...examinationData,
                          symptoms: e.target.value,
                        })
                      }
                    ></textarea>
                    <textarea
                      className="form-control"
                      rows="3"
                      placeholder="Kết luận chẩn đoán của bác sĩ..."
                      value={examinationData.diagnosis}
                      onChange={(e) =>
                        setExaminationData({
                          ...examinationData,
                          diagnosis: e.target.value,
                        })
                      }
                    ></textarea>
                  </div>

                  <hr />

                  {/* Chỉ định dịch vụ */}
                  <div className="mb-3">
                    <label className="form-label fw-bold">
                      Chỉ định dịch vụ
                    </label>
                    <div
                      className="bg-light p-3 rounded border"
                      style={{ maxHeight: '200px', overflowY: 'auto' }}
                    >
                      <div className="row">
                        {allServices.map((service) => (
                          <div key={service.serviceId} className="col-md-6">
                            <div className="form-check">
                              <input
                                className="form-check-input"
                                type="checkbox"
                                id={`service-${service.serviceId}`}
                                checked={examinationData.serviceIds.includes(
                                  service.serviceId
                                )}
                                onChange={() =>
                                  handleServiceChange(service.serviceId)
                                }
                              />
                              <label
                                className="form-check-label"
                                htmlFor={`service-${service.serviceId}`}
                              >
                                {service.serviceName}
                              </label>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>

                  <hr />

                  {/* Kê đơn thuốc */}
                  <div className="mb-3">
                    <label className="form-label fw-bold">
                      Kê đơn thuốc
                    </label>
                    {examinationData.prescriptions.map((row, index) => (
                      <div
                        key={index}
                        className="row g-2 align-items-center mb-2"
                      >
                        <div className="col-md-5">
                          <Select
                            className="form-select-sm"
                            options={medicineOptions}
                            value={medicineOptions.find(opt => opt.value === row.medicineId)}
                            onChange={(selected) =>
                              handlePrescriptionChange(index, "medicineId", selected.value)
                            }
                            placeholder="Tìm thuốc..."
                            isSearchable={true}
                          />
                        </div>
                        <div className="col-md-2">
                          <input
                            type="number"
                            className="form-control form-control-sm"
                            placeholder="SL"
                            value={row.quantity}
                            onChange={(e) =>
                              handlePrescriptionChange(
                                index,
                                'quantity',
                                e.target.value
                              )
                            }
                            min="1"
                          />
                        </div>
                        <div className="col-md-4">
                          <input
                            type="text"
                            className="form-control form-control-sm"
                            placeholder="Hướng dẫn sử dụng"
                            value={row.dosageInstruction}
                            onChange={(e) =>
                              handlePrescriptionChange(
                                index,
                                'dosageInstruction',
                                e.target.value
                              )
                            }
                          />
                        </div>
                        <div className="col-md-1">
                          <button
                            type="button"
                            className="btn btn-outline-danger btn-sm"
                            onClick={() => handleRemovePrescriptionRow(index)}
                          >
                            <Trash2 size={14} />
                          </button>
                        </div>
                      </div>
                    ))}
                    <button
                      type="button"
                      className="btn btn-success btn-sm mt-2 d-flex align-items-center gap-1"
                      onClick={handleAddPrescriptionRow}
                    >
                      <Plus size={14} /> Thêm thuốc
                    </button>
                  </div>

                  <hr />

                  <div className="d-flex justify-content-end gap-2">
                    <button
                      className="btn btn-secondary"
                      onClick={() => handleConfirmSubmit(false)}
                      disabled={isProcessing}
                    >
                      Tạm lưu
                    </button>
                    <button
                      className="btn btn-success"
                      onClick={() => handleConfirmSubmit(true)}
                      disabled={isProcessing}
                    >
                      Hoàn tất khám
                    </button>

                  </div>
                </div>
              </div>
            ) : (
              <div
                className="card shadow-sm border-0 d-flex justify-content-center align-items-center"
                style={{ minHeight: '300px' }}
              >
                <div className="text-center p-5">
                  <Stethoscope size={48} className="mx-auto text-muted" />
                  <p className="mt-3 text-muted">
                    Vui lòng chọn một bệnh nhân từ hàng chờ để bắt đầu khám.
                  </p>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {currentSection === 'schedule' && (
        <ScheduleSection events={scheduleEvents} isLoading={isLoading} />
      )}
      {currentSection === 'history' && <HistorySection />}

      <ConfirmDeleteModal
        isOpen={confirm.show}
        title={confirm.isComplete ? 'Xác nhận hoàn tất' : 'Xác nhận tạm lưu'}
        message={
          confirm.isComplete
            ? 'Bạn có chắc muốn hoàn tất và lưu vĩnh viễn hồ sơ khám bệnh này?'
            : 'Bạn có muốn lưu tạm thời thông tin đã nhập?'
        }
        onConfirm={processExaminationSubmit}
        onCancel={() => setConfirm({ show: false, isComplete: false })}
        confirmText="Xác nhận"
        isProcessing={isProcessing}
      />
    </main>
  );
};

export default DoctorDashboard;