import { useCallback, useEffect, useState } from 'react';
import AdminSidebar from '../../Components/Sidebar/AdminSidebar';
import Loading from '../../Components/Loading/Loading';
import CustomToast from '../../Components/CustomToast/CustomToast';
import instance from '../../axios';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import bootstrap5Plugin from '@fullcalendar/bootstrap5';
import 'bootstrap-icons/font/bootstrap-icons.css';
import { BiCalendarPlus, BiPencil, BiSave, BiTrash, BiX, BiXCircle } from 'react-icons/bi';
import { FaUserMd, FaUserNurse, FaUserPlus, FaUserTie } from 'react-icons/fa';
import '../../App.css';

const initialFormState = {
    staffId: '',
    workDate: new Date().toISOString().split('T')[0],
    startTime: '08:00:00',
    endTime: '17:00:00',
    isAvailable: true,
};

const AdminScheduleManagement = () => {
    const [schedules, setSchedules] = useState([]);
    const [staffList, setStaffList] = useState([]);
    const [loading, setLoading] = useState(true);
    const [toast, setToast] = useState(null);
    const [roomList, setRoomList] = useState([]);

    const [isFormModalOpen, setFormModalOpen] = useState(false);
    const [isDetailModalOpen, setDetailModalOpen] = useState(false);
    const [isConfirmModalOpen, setConfirmModalOpen] = useState(false);

    const [selectedEvent, setSelectedEvent] = useState(null);
    const [scheduleFormData, setScheduleFormData] = useState(initialFormState);

    const fetchRooms = useCallback(async () => {
        try {
            const response = await instance.get('Room/GetAllRooms');
            console.log("room ", response);
            const fetchedRooms = response.data || [];
            console.log("fetchedRooms ", fetchedRooms);
            setRoomList(fetchedRooms);
        } catch (error) {
            setToast({ type: 'error', message: error.message || 'Lỗi kết nối máy chủ.' })
        }
    }, []);

    useEffect(() => {
        fetchRooms();
    }, [fetchRooms]);
   
   
    

    const fetchData = useCallback(async () => {
        setLoading(true);
        try {
            const response = await instance.get('Schedule/GetAllSchedulesAsync');
            const fetchedSchedules = response.content.items || [];
            setSchedules(fetchedSchedules);
        } catch (error) {
            setToast({ type: 'error', message: error.message || 'Lỗi kết nối máy chủ.' });
        } finally {
            setLoading(false);
        }
    }, []);


    const fetchStaffList = useCallback(async () => {
        try {
            const response = await instance.get("Admin/GetAllMedicalStaffAsync");
            const doctors = response.content || [];
            setStaffList(doctors);
        } catch (error) {
            setToast({ type: 'error', message: error.message || 'Không tải được danh sách nhân viên.' });
        }
    }, []);

    useEffect(() => {
        fetchData();
        fetchStaffList();
    }, [fetchData, fetchStaffList]);


    const ChangeRole = (Role) => {
        switch (Role) {
            case 'Bác sĩ':
                return 'Doctor';
            case 'Y tá':
                return 'Nurse';
            case 'Lễ tân':
                return 'Receptionist';
            default:
                return Role;
        }
    };

    const calendarEvents = schedules.map(s => ({
        id: s.scheduleId,
        title: s.staffName,
        start: `${s.workDate}T${s.startTime}`,
        end: `${s.workDate}T${s.endTime}`,
        className: `event-${ChangeRole(s.role).toLowerCase().replace('ĩ', 'i').replace(/\s+/g, '')}`,
        extendedProps: { ...s }
    }));




    const handleCloseModals = () => {
        setFormModalOpen(false);
        setDetailModalOpen(false);
        setConfirmModalOpen(false);
        setSelectedEvent(null);
    };

    const handleEventClick = (clickInfo) => {
        setSelectedEvent(clickInfo.event);
        setDetailModalOpen(true);
    };

    const handleOpenAddModal = () => {
        setSelectedEvent(null);
        setScheduleFormData(initialFormState);
        setFormModalOpen(true);
    };

    const handleOpenEditModal = (event) => {
        handleCloseModals();
        const { extendedProps } = event;
        setScheduleFormData({
            staffId: extendedProps.staffId,
            workDate: extendedProps.workDate,
            startTime: extendedProps.startTime,
            endTime: extendedProps.endTime,
            roomId: extendedProps.roomId,
            isAvailable: extendedProps.isAvailable,
        });
        setSelectedEvent(event);
        setFormModalOpen(true);
    };

    const handleOpenDeleteModal = (event) => {
        setDetailModalOpen(false);
        setSelectedEvent(event);
        setConfirmModalOpen(true);
    };

    const handleFormChange = (e) => {
        const { name, value, type, checked } = e.target;
        setScheduleFormData(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
    };

    const handleFormSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        const isEditing = !!selectedEvent;
        const url = isEditing ? `Schedule/UpdateScheduleAsync/${selectedEvent.id}` : 'Schedule/CreateScheduleAsync';
        const method = isEditing ? 'put' : 'post';
        try {
            const response = await instance[method](url, scheduleFormData);
            setToast({ type: 'success', message: response.message });
            handleCloseModals();
            fetchData();
        } catch (error) {
            setToast({ type: 'error', message: error.response?.data?.message || 'Thao tác thất bại.' });
        } finally {
            setLoading(false);
        }
    };

    const handleDeleteConfirm = async () => {
        if (!selectedEvent) return;
        setLoading(true);
        try {
            const response = await instance.delete(`Schedule/DeleteScheduleAsync/${selectedEvent.id}`);
            setToast({ type: 'success', message: response.message });
            handleCloseModals();
            fetchData();
        } catch (error) {
            setToast({ type: 'error', message: error.response?.data?.message || 'Lỗi khi xóa.' });
        } finally {
            setLoading(false);
        }
    };

    const renderEventContent = (eventInfo) => {
        const { Role } = eventInfo.event.extendedProps;
        const roleIcons = { 'Bác sĩ': <FaUserMd />, 'Y tá': <FaUserNurse />, 'Lễ tân': <FaUserPlus /> };
        return (
            <div className="event-main-content w-100">
                <div className="event-icon">{roleIcons[Role] || <FaUserTie />}</div>
                <div className="event-details">
                    <div className="event-title">{eventInfo.event.title}</div>
                    <div className="event-role">{Role}</div>
                </div>
            </div>
        );
    };

    // Hàm render các Modal
    const renderModals = () => (
        <>
            {/* MODAL THÊM / SỬA LỊCH */}
            {isFormModalOpen && (
                <div className="modal-backdrop fade show"></div>
            )}
            <div className={`modal fade ${isFormModalOpen ? 'show d-block' : ''}`} tabIndex="-1" onClick={handleCloseModals}>
                <div className="modal-dialog modal-dialog-centered" onClick={e => e.stopPropagation()}>
                    <div className="modal-content">
                        <div className="modal-header"><h5 className="modal-title">{selectedEvent ? "Cập Nhật Lịch" : "Thêm Lịch Mới"}</h5><button type="button" className="btn-close" onClick={handleCloseModals}></button></div>
                        <form onSubmit={handleFormSubmit}>
                            <div className="modal-body">
                                <select name="staffId" value={scheduleFormData.staffId} onChange={handleFormChange} className="form-select" required disabled={!!selectedEvent}>
                                    <option value="">-- Chọn nhân viên --</option>
                                    {staffList.map(staff => (
                                        <option key={staff.staffId} value={staff.staffId}>
                                            {staff.staffName} ({staff.staffType})
                                        </option>
                                    ))}
                                </select>

                                <div className="mb-3"><label className="form-label">Ngày làm</label><input type="date" name="workDate" value={scheduleFormData.workDate} onChange={handleFormChange} className="form-control" required /></div>
                                <div className="mb-3"><label className="form-label">Phòng</label><select name="roomId" value={scheduleFormData.roomId} onChange={handleFormChange} className="form-select" required ><option value="">-- Chọn phòng --</option>{roomList.map(room => <option key={room.roomId} value={room.roomId}>{room.roomName}</option>)}</select></div>
                                <div className="row mb-3">
                                    <div className="col-md-6"><label className="form-label">Bắt đầu</label><input type="time" name="startTime" value={scheduleFormData.startTime} onChange={handleFormChange} className="form-control" step="1" required /></div>
                                    <div className="col-md-6"><label className="form-label">Kết thúc</label><input type="time" name="endTime" value={scheduleFormData.endTime} onChange={handleFormChange} className="form-control" step="1" required /></div>
                                </div>
                                <div className="form-check form-switch"><input type="checkbox" name="isAvailable" checked={scheduleFormData.isAvailable} onChange={handleFormChange} className="form-check-input" id="isAvailableCheck" /><label className="form-check-label" htmlFor="isAvailableCheck">Có mặt</label></div>
                            </div>
                            <div className="modal-footer"><button type="button" className="btn btn-secondary" onClick={handleCloseModals}>Hủy</button><button type="submit" className="btn btn-primary"><BiSave /> Lưu</button></div>
                        </form>
                    </div>
                </div>
            </div>

            {/* MODAL XEM CHI TIẾT */}
            {isDetailModalOpen && (
                <div className="modal-backdrop fade show"></div>
            )}
            <div className={`modal fade ${isDetailModalOpen && selectedEvent ? 'show d-block' : ''}`} tabIndex="-1" onClick={handleCloseModals}>
                <div className="modal-dialog modal-dialog-centered" onClick={e => e.stopPropagation()}>
                    <div className="modal-content">
                        <div className="modal-header"><h5 className="modal-title">Chi Tiết Lịch Làm Việc</h5><button type="button" className="btn-close" onClick={handleCloseModals}></button></div>
                        <div className="modal-body">
                            <div className="info-row"><span className="label">Nhân viên:</span> <span className="value">{selectedEvent?.extendedProps.staffName}</span></div>
                            <div className="info-row"><span className="label">Chức vụ:</span> <span className="value">{selectedEvent?.extendedProps.role}</span></div>
                            <div className="info-row"><span className='label'>Phòng trực: </span> <span className="value">{selectedEvent?.extendedProps.roomName}</span></div>
                            <div className="info-row"><span className="label">Ngày làm:</span> <span className="value">{selectedEvent ? new Date(selectedEvent.startStr).toLocaleDateString('vi-VN') : ''}</span></div>
                            <div className="info-row"><span className="label">Thời gian:</span> <span className="value">{selectedEvent ? `${new Date(selectedEvent.startStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - ${new Date(selectedEvent.endStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}` : ''}</span></div>
                            <div className="info-row"><span className="label">Trạng thái:</span> <span className="value">{selectedEvent?.extendedProps.isAvailable ? "Có mặt" : "Vắng"}</span></div>
                        </div>
                        <div className="modal-footer d-flex justify-content-end gap-2"><button className="btn btn-outline-secondary" onClick={() => handleOpenEditModal(selectedEvent)}><BiPencil /> Sửa</button><button className="btn btn-outline-danger" onClick={() => handleOpenDeleteModal(selectedEvent)}><BiTrash /> Xóa</button><button className="btn btn-primary" onClick={handleCloseModals}><BiX /> Đóng</button></div>
                    </div>
                </div>
            </div>

            {/* MODAL XÁC NHẬN XÓA */}
            {isConfirmModalOpen && (
                <div className="modal-backdrop fade show"></div>
            )}
            <div className={`modal fade ${isConfirmModalOpen && selectedEvent ? 'show d-block' : ''}`} tabIndex="-1" onClick={handleCloseModals}>
                <div className="modal-dialog modal-dialog-centered modal-sm" onClick={e => e.stopPropagation()}>
                    <div className="modal-content">
                        <div className="modal-header"><h5 className="modal-title">Xác Nhận Xóa</h5><button type="button" className="btn-close" onClick={handleCloseModals}></button></div>
                        <div className="modal-body"><p>Bạn có chắc muốn xóa lịch của <strong>{selectedEvent?.extendedProps.staffName}</strong> vào ngày <strong>{selectedEvent ? new Date(selectedEvent.startStr).toLocaleDateString('vi-VN') : ''}</strong>?</p></div>
                        <div className="modal-footer"><button className="btn btn-secondary" onClick={handleCloseModals}>Hủy</button><button className="btn btn-danger" onClick={handleDeleteConfirm}>Xác Nhận</button></div>
                    </div>
                </div>
            </div>
        </>
    );

    return (
        <div className="d-flex w-100">
            <main className="main-content flex-grow-1 p-4 d-flex flex-column gap-4">
                {toast && <CustomToast type={toast.type} message={toast.message} onClose={() => setToast(null)} />}
                <header className="d-flex justify-content-between align-items-center flex-shrink-0">
                    <h1 className="h4 mb-0 fw-bold">Quản Lý Lịch Làm Việc</h1>
                    <div className="d-flex align-items-center gap-3">
                        <div className="legend d-flex align-items-center gap-2">
                            <span className="badge bg-doctor">Bác sĩ</span><span className="badge bg-nurse">Y tá</span>
                            <span className="badge bg-receptionist">Lễ tân</span>
                        </div>
                        <button className="btn btn-primary d-flex align-items-center gap-2" onClick={handleOpenAddModal}>
                            <BiCalendarPlus /> Thêm Lịch Mới
                        </button>
                    </div>
                </header>

                <div className="card shadow-sm border-0 calendar-panel p-3">
                    {loading && <Loading isLoading={loading} />}
                    <div className="flex-grow-1" style={{ visibility: loading ? 'hidden' : 'visible' }}>
                        <FullCalendar
                            plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin, bootstrap5Plugin]}
                            themeSystem="bootstrap5"
                            headerToolbar={{ left: 'prev,next today', center: 'title', right: 'dayGridMonth,timeGridDay' }}
                            initialView="dayGridMonth"
                            locale="vi"
                            height="100%"
                            events={calendarEvents}
                            eventClick={handleEventClick}
                            eventContent={renderEventContent}
                            buttonText={{ today: 'Hôm nay', month: 'Tháng', day: 'Ngày' }}
                        />
                    </div>
                </div>

                {renderModals()}
            </main>
        </div>
    );
};

export default AdminScheduleManagement;