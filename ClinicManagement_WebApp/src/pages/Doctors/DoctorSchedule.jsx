import { useCallback, useEffect, useState } from 'react';
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


const DoctorSchedule = () => {
    const [schedules, setSchedules] = useState([]);
    const [loading, setLoading] = useState(true);
    const [toast, setToast] = useState(null);
    const [isDetailModalOpen, setDetailModalOpen] = useState(false);

    const [selectedEvent, setSelectedEvent] = useState(null);



    const fetchData = useCallback(async () => {
        setLoading(true);
        try {
            const response = await instance.get('Doctor/GetMySchedule');
            console.log(response);

            const fetchedSchedules = response.content || [];
            setSchedules(fetchedSchedules);
        } catch (error) {
            setToast({ type: 'error', message: error.message || 'Lỗi kết nối máy chủ.' });
        } finally {
            setLoading(false);
        }
    }, []);



    useEffect(() => {
        fetchData();
    }, [fetchData]);


    const handleCloseDetailModal = () => {
        setDetailModalOpen(false);
        setSelectedEvent(null);
    };

    const handleEventClick = (clickInfo) => {
        setSelectedEvent(clickInfo.event);
        setDetailModalOpen(true);
    };




    const renderEventContent = (eventInfo) => {
        return (
            <div className="event-main-content w-100">
                <div className="event-icon"><FaUserMd /></div>
                <div className="event-details">
                    <div className="event-title">{eventInfo.event.title} - Room {eventInfo.event.extendedProps.roomId}</div>
                    <div className="event-role">Doctor</div>
                </div>
            </div>
        );
    };

    // Hàm render các Modal
    const renderModals = () => (
        <>
            {/* MODAL XEM CHI TIẾT */}
            {isDetailModalOpen && (
                <div className="modal-backdrop fade show"></div>
            )}
            <div className={`modal fade ${isDetailModalOpen && selectedEvent ? 'show d-block' : ''}`} tabIndex="-1" onClick={handleCloseDetailModal}>
                <div className="modal-dialog modal-dialog-centered" onClick={e => e.stopPropagation()}>
                    <div className="modal-content">
                        <div className="modal-header"><h5 className="modal-title">Chi Tiết Lịch Làm Việc</h5><button type="button" className="btn-close" onClick={handleCloseDetailModal}></button></div>
                        <div className="modal-body">
                            <div className="info-row"><span className="label">Nhân viên:</span> <span className="value">{selectedEvent?.extendedProps.staffName}</span></div>
                            <div className="info-row"><span className="label">Chức vụ:</span> <span className="value">{selectedEvent?.extendedProps.role}</span></div>
                            <div className="info-row"><span className='label'>Phòng trực: </span> <span className="value">{selectedEvent?.extendedProps.roomId}</span></div>
                            <div className="info-row"><span className="label">Ngày làm:</span> <span className="value">{selectedEvent ? new Date(selectedEvent.startStr).toLocaleDateString('vi-VN') : ''}</span></div>
                            <div className="info-row"><span className="label">Thời gian:</span> <span className="value">{selectedEvent ? `${new Date(selectedEvent.startStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - ${new Date(selectedEvent.endStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}` : ''}</span></div>
                            <div className="info-row"><span className="label">Trạng thái:</span> <span className="value">{selectedEvent?.extendedProps.isAvailable ? "Có mặt" : "Vắng"}</span></div>
                        </div>
                        <div className="modal-footer d-flex justify-content-end gap-2"><button className="btn btn-primary" onClick={handleCloseDetailModal}><BiX /> Đóng</button></div>
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
                    <h1 className="h4 mb-0 fw-bold">Lịch Làm Việc</h1>
                </header>

                <div className="card shadow-sm border-0 calendar-panel p-3">
                    {loading ? (
                        <div className="d-flex justify-content-center align-items-center" style={{ height: '500px' }}>
                            <Loading isLoading={loading} />
                        </div>
                    ) : (
                        <FullCalendar
                            plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin, bootstrap5Plugin]}
                            themeSystem="bootstrap5"
                            headerToolbar={{
                                left: 'prev,next today',
                                center: 'title',
                                right: 'dayGridMonth,timeGridWeek,timeGridDay'
                            }}
                            initialView="dayGridMonth"
                            locale="vi"
                            height="auto"
                            minHeight="600px"
                            events={schedules.map(s => ({
                                id: s.scheduleId,
                                title: s.staffName,
                                start: `${s.workDate}T${s.startTime}`,
                                end: `${s.workDate}T${s.endTime}`,
                                extendedProps: s,
                                className: `event-doctor`
                            }))}
                            eventClick={handleEventClick}
                            eventContent={renderEventContent}
                            buttonText={{
                                today: 'Hôm nay',
                                month: 'Tháng',
                                week: 'Tuần',
                                day: 'Ngày'
                            }}
                        />
                    )}
                </div>

                {renderModals()}
            </main>
        </div>
    );
};

export default DoctorSchedule;