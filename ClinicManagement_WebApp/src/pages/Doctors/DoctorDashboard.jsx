import React, { useState, useEffect, useCallback, memo, useRef } from 'react';
import { Calendar, CheckSquare, Clock, List, Stethoscope, User, X, Printer, Plus, Trash2, Search } from 'lucide-react';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from "@fullcalendar/interaction"; // Dành cho các tương tác trên lịch
import Loading from '../../Components/Loading/Loading';
import CustomToast from '../../Components/CustomToast/CustomToast';
import ConfirmDeleteModal from '../../Components/CustomToast/DeleteConfirmModal';
import Pagination from '../../Components/Pagination/Pagination';
import instance from '../../axios';
import '../../App.css';

// --- Helper Functions ---
const formatVND = (value) => {
  if (value === null || value === undefined) return 'N/A';
  return Number(value).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
};
const formatDate = (dateString) => {
  if (!dateString) return 'N/A';
  return new Date(dateString).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

// --- COMPONENT CON 1: Sidebar ---
const DoctorSidebar = memo(({ currentSection, switchSection }) => {
  const navItems = [
    { id: 'today', icon: <Clock size={20} />, text: 'Lịch khám hôm nay' },
    { id: 'schedule', icon: <Calendar size={20} />, text: 'Lịch làm việc' },
    { id: 'history', icon: <List size={20} />, text: 'Lịch sử bệnh nhân' },
  ];
  return (
    <nav className="sidebar">
      <div className="sidebar-header p-4 text-center border-bottom border-white border-opacity-25">
        <h4 className="h5">Bác Sĩ</h4>
        <p className="text-light mb-0 small">Bảng điều khiển</p>
      </div>
      <ul className="nav-list">
        {navItems.map(item => (
          <li key={item.id}>
            <a href="#" className={`nav-item ${currentSection === item.id ? 'active' : ''}`} onClick={(e) => { e.preventDefault(); switchSection(item.id); }}>
              {item.icon}
              <span>{item.text}</span>
            </a>
          </li>
        ))}
      </ul>
    </nav>
  );
});

// --- COMPONENT CON 2: Lịch làm việc (FullCalendar) ---
const ScheduleSection = memo(({ events, isLoading }) => (
  <div className="card shadow-sm border-0 table-panel">
    {isLoading ? <Loading isLoading={true} /> : (
      <div className="card-body calendar-panel p-4">
        <FullCalendar
          plugins={[dayGridPlugin, interactionPlugin]}
          initialView="dayGridMonth"
          events={events}
          locale='vi'
          headerToolbar={{
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,dayGridWeek'
          }}
          eventBackgroundColor="#0d6efd"
          eventBorderColor="#0d6efd"
        />
      </div>
    )}
  </div>
));

// --- COMPONENT CON 3: Lịch sử bệnh nhân ---
const HistorySection = memo(() => {
  // Logic cho lịch sử bệnh nhân có thể rất phức tạp, 
  // tạm thời giữ placeholder để tập trung vào luồng chính.
  return (
    <div className="card shadow-sm border-0">
      <div className="card-body text-center p-5">
        <List size={48} className="mx-auto text-muted" />
        <h5 className="mt-3 text-muted">Chức năng Lịch sử bệnh nhân đang được phát triển.</h5>
      </div>
    </div>
  );
});

// --- COMPONENT CHÍNH: DoctorDashboard ---
const DoctorDashboard = () => {
  const [currentSection, setCurrentSection] = useState('today');
  const [queuePatients, setQueuePatients] = useState([]); // << Đổi tên từ todayPatients
  const [scheduleEvents, setScheduleEvents] = useState([]);
  const [allServices, setAllServices] = useState([]);
  const [allMedicines, setAllMedicines] = useState([]);
  const [selectedPatient, setSelectedPatient] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [toast, setToast] = useState(null);
  const [confirm, setConfirm] = useState({ show: false, isComplete: false });
  const [isProcessing, setIsProcessing] = useState(false);

  // State cho toàn bộ form khám bệnh
  const [examinationData, setExaminationData] = useState({
    symptoms: '',
    diagnosis: '',
    prescriptions: [],
    serviceIds: [],
  });
  const ROOM_ID = 1; 
  const showToast = (type, message) => setToast({ type, message });

  // API: Lấy danh sách bệnh nhân khám hôm nay
  const fetchTodayPatients = useCallback(async () => {
    setIsLoading(true);
    try {
      const today = new Date().toLocaleDateString('en-CA'); 
      const res = await instance.get('Doctor/GetTodaysAppointmentsAsync', { params: { date: today } });
      setTodayPatients(res.content || []);
    } catch (error) { showToast('error', 'Lỗi khi tải lịch khám hôm nay.'); }
    finally { setIsLoading(false); }
  }, []);

  // API: Lấy hàng chờ của phòng khám
  const fetchQueue = useCallback(async () => {
    setIsLoading(true);
    try {
      const today = new Date().toLocaleDateString('en-CA'); 
      const res = await instance.get(`Queue/queues/room/${ROOM_ID}`, { params: { date: today } });
      console.log(res,today);
      setQueuePatients(res.data || []);
    } catch (error) { showToast('error', 'Lỗi khi tải hàng chờ.'); }
    finally { setIsLoading(false); }
  }, [ROOM_ID]);
  // API: Lấy danh sách dịch vụ và thuốc
  const fetchServicesAndMedicines = useCallback(async () => {
    if (allServices.length > 0 && allMedicines.length > 0) return;
    try {
      const [servicesRes, medicinesRes] = await Promise.all([
        instance.get('Doctor/GetAllServicesByDoctorAsync'),
        instance.get('Doctor/GetAllMedicinesByDoctorAsync')
      ]);
      setAllServices(servicesRes.content?.items || []);
      setAllMedicines(medicinesRes.content?.items || []);
    } catch (error) {
      showToast('error', 'Lỗi khi tải danh sách dịch vụ/thuốc.');
    }
  }, [allServices.length, allMedicines.length]);
  // API: Lấy lịch làm việc của bác sĩ (đã đăng nhập)
  const fetchSchedule = useCallback(async () => {
    setIsLoading(true);
    try {
      const res = await instance.get('Doctor/GetMySchedule');
      const formattedEvents = (res.content || []).map(item => ({
        title: item.patientName,
        start: `${item.appointmentDate}T${item.appointmentTime}`,
        extendedProps: item,
      }));
      setScheduleEvents(formattedEvents);
    } catch (error) { showToast('error', 'Lỗi khi tải lịch làm việc.'); }
    finally { setIsLoading(false); }
  }, []);

  // Effect để tải dữ liệu khi chuyển section
  useEffect(() => {
    if (currentSection === 'today') {
      fetchQueue();
      fetchServicesAndMedicines();
    }
    else if (currentSection === 'schedule') fetchSchedule();
    setSelectedPatient(null);
  }, [currentSection, fetchQueue, fetchSchedule, fetchServicesAndMedicines]);

  // Xử lý khi chọn một bệnh nhân từ hàng chờ
  const handleSelectPatient = async (patient) => {
    // Nếu bệnh nhân đang chờ, gọi API để bắt đầu khám
    if (patient.status === 'Waiting') {
      try {
        setIsLoading(true);
        await instance.put(`Queue/start/${patient.queueId}`);
        showToast('info', `Bắt đầu khám cho bệnh nhân ${patient.patientName}`);
        // Tải lại hàng chờ để cập nhật trạng thái cho tất cả mọi người
        fetchQueue();
      } catch (error) {
        showToast('error', 'Không thể bắt đầu phiên khám.');
        return;
      } finally {
        setIsLoading(false);
      }
    }
    setSelectedPatient(patient);
    setExaminationData({ symptoms: '', diagnosis: '', prescriptions: [], serviceIds: [] });
  };

  // Mở modal xác nhận
  const handleConfirmSubmit = (isComplete) => { setConfirm({ show: true, isComplete }); };

  // API: Gửi toàn bộ dữ liệu khám bệnh
  const processExaminationSubmit = async () => { /* ... Giữ nguyên ... */ };

  // --- Các hàm xử lý cho form con ---
  const handleServiceChange = (serviceId) => {
    setExaminationData(prev => {
      const newServiceIds = prev.serviceIds.includes(serviceId)
        ? prev.serviceIds.filter(id => id !== serviceId)
        : [...prev.serviceIds, serviceId];
      return { ...prev, serviceIds: newServiceIds };
    });
  };

  const handleAddPrescriptionRow = () => {
    setExaminationData(prev => ({
      ...prev,
      prescriptions: [...prev.prescriptions, { medicineId: '', quantity: 1, dosageInstruction: '' }]
    }));
  };

  const handleRemovePrescriptionRow = (index) => {
    setExaminationData(prev => ({
      ...prev,
      prescriptions: prev.prescriptions.filter((_, i) => i !== index)
    }));
  };

  const handlePrescriptionChange = (index, field, value) => {
    setExaminationData(prev => {
      const newPrescriptions = [...prev.prescriptions];
      newPrescriptions[index][field] = value;
      return { ...prev, prescriptions: newPrescriptions };
    });
  };

  return (
    <div className="d-flex">
      <main className="main-content flex-grow-1 p-4 d-flex flex-column gap-4">
        {toast && <CustomToast type={toast.type} message={toast.message} onClose={() => setToast(null)} />}

        <header className="d-flex justify-content-between align-items-center flex-shrink-0"><h1 className="h4 mb-0">Bảng Điều Khiển Bác Sĩ</h1></header>

        {isLoading && queuePatients.length === 0 && <Loading isLoading={true} />}

        {!isLoading && currentSection === 'today' && (
          <div className="row g-4">
            {/* Cột hàng chờ */}
            <div className="col-lg-4">
              <div className="card shadow-sm border-0 h-100">
                <div className="card-header fw-bold bg-light">Hàng chờ khám ({formatDate(new Date())})</div>
                <div className="list-group list-group-flush" style={{ maxHeight: '75vh', overflowY: 'auto' }}>
                  {queuePatients.length > 0 ? queuePatients.filter(p=>p.status!=='Completed').map(p => (
                    <a href="#" key={p.queueId} onClick={(e) => { e.preventDefault(); handleSelectPatient(p); }}
                      className={`list-group-item list-group-item-action d-flex justify-content-between align-items-start ${selectedPatient?.queueId === p.queueId ? 'active' : ''}`}>
                      <div className="ms-2 me-auto">
                        <div className="fw-bold">{p.queueNumber}. {p.patientName}</div>
                        <small>Giờ vào chờ: {p.queueTime}</small>
                      </div>
                      <span className={`badge rounded-pill ${p.status === 'Completed' ? 'bg-success-soft' : p.status === 'In Progress' ? 'bg-info-soft' : 'bg-warning-soft'}`}>
                        {p.status}
                      </span>
                    </a>
                  )) : <div className="text-center p-5 text-muted">Không có bệnh nhân trong hàng chờ.</div>}
                </div>
              </div>
            </div>
            {/* Cột form khám bệnh */}
            <div className="col-lg-8">
              {selectedPatient ? (
                <div className="card shadow-sm border-0">
                  <div className="card-header bg-light d-flex justify-content-between align-items-center">
                    <span className="fw-bold">Bệnh án: {selectedPatient.patientName} (Số {selectedPatient.queueNumber})</span>
                    <button className="btn-close" onClick={() => setSelectedPatient(null)}></button>
                  </div>
                  <div className="card-body p-4">
                    {/* Chẩn đoán */}
                    <div className="mb-3"><label className="form-label fw-bold">Chẩn đoán</label>
                      <textarea className="form-control mb-2" rows="3" placeholder="Triệu chứng của bệnh nhân..." value={examinationData.symptoms} onChange={e => setExaminationData({ ...examinationData, symptoms: e.target.value })}></textarea>
                      <textarea className="form-control" rows="3" placeholder="Kết luận chẩn đoán của bác sĩ..." value={examinationData.diagnosis} onChange={e => setExaminationData({ ...examinationData, diagnosis: e.target.value })}></textarea>
                    </div>
                    <hr />
                    {/* Chỉ định dịch vụ */}
                    <div className="mb-3">
                      <label className="form-label fw-bold">Chỉ định dịch vụ</label>
                      <div className="bg-light p-3 rounded border" style={{ maxHeight: '200px', overflowY: 'auto' }}>
                        <div className="row">
                          {allServices.map(service => (
                            <div key={service.serviceId} className="col-md-6">
                              <div className="form-check">
                                <input className="form-check-input" type="checkbox" id={`service-${service.serviceId}`}
                                  checked={examinationData.serviceIds.includes(service.serviceId)}
                                  onChange={() => handleServiceChange(service.serviceId)} />
                                <label className="form-check-label" htmlFor={`service-${service.serviceId}`}>{service.serviceName}</label>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    </div>
                    <hr />
                    {/* Kê đơn thuốc */}
                    <div className="mb-3">
                      <label className="form-label fw-bold">Kê đơn thuốc</label>
                      {examinationData.prescriptions.map((row, index) => (
                        <div key={index} className="row g-2 align-items-center mb-2">
                          <div className="col-md-5"><select className="form-select form-select-sm" value={row.medicineId} onChange={(e) => handlePrescriptionChange(index, 'medicineId', e.target.value)}><option value="">Chọn thuốc</option>{allMedicines.map(m => <option key={m.medicineId} value={m.medicineId}>{m.medicineName}</option>)}</select></div>
                          <div className="col-md-2"><input type="number" className="form-control form-control-sm" placeholder="SL" value={row.quantity} onChange={(e) => handlePrescriptionChange(index, 'quantity', e.target.value)} min="1" /></div>
                          <div className="col-md-4"><input type="text" className="form-control form-control-sm" placeholder="Hướng dẫn sử dụng" value={row.dosageInstruction} onChange={(e) => handlePrescriptionChange(index, 'dosageInstruction', e.target.value)} /></div>
                          <div className="col-md-1"><button type="button" className="btn btn-outline-danger btn-sm" onClick={() => handleRemovePrescriptionRow(index)}><Trash2 size={14} /></button></div>
                        </div>
                      ))}
                      <button type="button" className="btn btn-success btn-sm mt-2 d-flex align-items-center gap-1" onClick={handleAddPrescriptionRow}><Plus size={14} /> Thêm thuốc</button>
                    </div>
                    <hr />
                    <div className="d-flex justify-content-end gap-2">
                      <button className="btn btn-secondary" onClick={() => handleConfirmSubmit(false)} disabled={isProcessing}>Tạm lưu</button>
                      <button className="btn btn-success" onClick={() => handleConfirmSubmit(true)} disabled={isProcessing}>Hoàn tất khám</button>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="card shadow-sm border-0 d-flex justify-content-center align-items-center" style={{ minHeight: '300px' }}>
                  <div className="text-center p-5"><Stethoscope size={48} className="mx-auto text-muted" /><p className="mt-3 text-muted">Vui lòng chọn một bệnh nhân từ hàng chờ để bắt đầu khám.</p></div>
                </div>
              )}
            </div>
          </div>
        )}

        {!isLoading && currentSection === 'schedule' && <ScheduleSection events={scheduleEvents} isLoading={isLoading} />}
        {!isLoading && currentSection === 'history' && <HistorySection />}

        <ConfirmDeleteModal isOpen={confirm.show} title={confirm.isComplete ? "Xác nhận hoàn tất" : "Xác nhận tạm lưu"} message={confirm.isComplete ? "Bạn có chắc muốn hoàn tất và lưu vĩnh viễn hồ sơ khám bệnh này?" : "Bạn có muốn lưu tạm thời thông tin đã nhập?"} onConfirm={processExaminationSubmit} onCancel={() => setConfirm({ show: false, isComplete: false })} confirmText="Xác nhận" isProcessing={isProcessing} />
      </main>
    </div>
  );
};

export default DoctorDashboard;