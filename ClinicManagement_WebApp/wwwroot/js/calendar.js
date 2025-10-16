// Biến global để lưu calendar instance
let currentCalendar = null;
let dotNetHelper = null;
// Hàm kiểm tra FullCalendar đã load chưa
function isFullCalendarLoaded() {
    const loaded = typeof FullCalendar !== 'undefined' && FullCalendar.Calendar !== 'undefined';
    console.log('Checking FullCalendar status:', loaded);
    return loaded;
}

// Hàm chờ FullCalendar load
function waitForFullCalendar(callback, maxRetries = 100, interval = 200) {
    let retries = 0;
    function check() {
        console.log(`Attempt ${retries + 1}/${maxRetries} to load FullCalendar`);
        if (isFullCalendarLoaded()) {
            console.log('FullCalendar loaded successfully');
            callback();
        } else if (retries < maxRetries) {
            retries++;
            setTimeout(check, interval);
        } else {
            console.error('FullCalendar failed to load after maximum retries');
            const calendarEl = document.getElementById('calendar');
            if (calendarEl) {
                calendarEl.innerHTML = '<div class="alert alert-danger">Lỗi: Không thể tải lịch. Vui lòng làm mới trang.</div>';
            }
        }
    }
    check();
}

// Hàm render calendar với error handling
window.renderScheduleCalendar = (data) => {
    console.log('renderScheduleCalendar called with events:', data);
    const calendarEl = document.getElementById('calendar');
    if (!calendarEl) {
        console.error('Calendar element not found');
        return;
    }

    // Xóa nội dung cũ
    calendarEl.innerHTML = "";

    // Kiểm tra FullCalendar đã load chưa
    if (!isFullCalendarLoaded()) {
        console.error('FullCalendar is not loaded yet. Retrying...');
        setTimeout(() => window.renderScheduleCalendar(data), 500);
        return;
    }

    try {
        // Hủy calendar cũ nếu có
        if (currentCalendar) {
            currentCalendar.destroy();
            console.log('Destroyed previous calendar instance');
        }

        // Xử lý events undefined hoặc rỗng
        let events = (data && data.events) ? data.events : [];
    dotNetHelper = (data && data.dotNetHelper) ? data.dotNetHelper : null;

        console.log('Processed events array length:', events.length);

        // Xác định initialDate từ sự kiện đầu tiên nếu có
        let initialDate = events.length > 0 ? events[0].start.split('T')[0] : new Date().toISOString().split('T')[0];
        console.log('Initial date set to:', initialDate);

        // Tạo calendar mới
        currentCalendar = new FullCalendar.Calendar(calendarEl, {
            themeSystem: 'bootstrap5',
            locale: 'vi',
            initialView: 'dayGridMonth',
            initialDate: initialDate,
            timeZone: 'local',
            height: 'auto',
            events: events,
            showNonCurrentDates: false,
            fixedWeekCount: false,
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay'
            },
            events: events,
            eventContent: function (info) {
                // Áp dụng cho chế độ xem Tháng
                if (info.view.type === 'dayGridMonth') {
                    return { html: info.event.title };
                }

                // Áp dụng cho chế độ xem Tuần/Ngày (timeGrid)
                if (info.view.type.startsWith('timeGrid')) {
                    // Tách chuỗi title của bạn dựa trên thẻ <br>
                    // Ví dụ: "Đặng Quang Minh (Doctor)<br>08:00 - 17:00"
                    const titleParts = info.event.title.split('<br>');
                    const mainTitle = titleParts[0]; // "Đặng Quang Minh (Doctor)"
                    const timeText = titleParts.length > 1 ? titleParts[1] : ''; // "08:00 - 17:00"

                    // Tạo HTML tùy chỉnh
                    let customHtml = `
                <div class="fc-event-main-custom">
                    <div class="fc-event-title-custom">
                        <i class="bi bi-person-fill"></i> ${mainTitle}
                    </div>
                    <div class="fc-event-time-custom">
                        <i class="bi bi-clock"></i> ${timeText}
                    </div>
                </div>  
            `;
                    return { html: customHtml };
                }

                // Mặc định cho các view khác
                return true;
            },
         eventClick: function(info) {
                console.log('Event clicked, ID:', info.event.id);

                // Logic đúng: Kiểm tra xem dotNetHelper có tồn tại không
                if (dotNetHelper) {
                    // Tên biến đúng, và chuyển ID sang string để đảm bảo an toàn
                    dotNetHelper.invokeMethodAsync('HandleEventClick', info.event.id.toString())
                        .catch(err => console.error("Error invoking C# method 'HandleEventClick'", err));
                } else {
                    console.error('DotNet helper not available. Cannot call C#.');
                    alert('Lỗi kết nối, không thể xem chi tiết.');
                }
            },
            eventDidMount: (info) => {
                console.log('Event mounted:', info.event);
                info.el.style.backgroundColor = info.event.backgroundColor;
                info.el.style.borderColor = info.event.borderColor;
                info.el.style.color = info.event.textColor;
            },
            loading: function (isLoading) {
                console.log('Calendar loading:', isLoading);
            }
        });

        currentCalendar.render();
        console.log('Calendar rendered with events:', events.length);
    } catch (error) {
        console.error('Error creating calendar:', error);
        calendarEl.innerHTML = '<div class="alert alert-danger">Lỗi tải lịch: ' + error.message + '</div>';
    }
};

// Khởi tạo khi DOM ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, checking scripts...');
    console.log('Blazor:', window.Blazor ? 'Loaded' : 'Not loaded');
    console.log('FullCalendar:', isFullCalendarLoaded() ? 'Loaded' : 'Not loaded');
    waitForFullCalendar(() => {
        console.log('FullCalendar loaded successfully');
    });
});