
window.initializeChart = function (canvasId) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) {
        console.error('Không tìm thấy canvas element với id:', canvasId);
        return;
    }

    // Nếu biểu đồ đã tồn tại, hủy nó đi trước khi tạo mới
    if (window[canvasId] instanceof Chart) {
        window[canvasId].destroy();
    }

    window[canvasId] = new Chart(ctx, {
        type: 'bar', // Kiểu biểu đồ cột
        data: {
            labels: [], // Dữ liệu nhãn (ngày tháng) sẽ được cập nhật sau
            datasets: [{
                label: 'Doanh Thu (VNĐ)', // Tên của cột dữ liệu
                data: [], // Dữ liệu (số tiền) sẽ được cập nhật sau
                backgroundColor: 'rgba(23, 162, 184, 0.6)', // Màu nền của cột
                borderColor: 'rgba(23, 162, 184, 1)',   // Màu viền của cột
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        // Định dạng lại số trên trục Y cho dễ đọc (vd: 1,000,000)
                        callback: function (value, index, values) {
                            return new Intl.NumberFormat('vi-VN').format(value);
                        }
                    }
                }
            }
        }
    });
};

// Hàm này nhận dữ liệu từ Blazor và cập nhật lại biểu đồ
window.updateChart = function (canvasId, newLabels, newData) {
    const chart = window[canvasId];
    if (chart) {
        chart.data.labels = newLabels;
        chart.data.datasets[0].data = newData;
        chart.update(); // "Vẽ" lại biểu đồ với dữ liệu mới
    } else {
        console.error('Biểu đồ chưa được khởi tạo:', canvasId);
    }
};

window.downloadCsv = function (filename, csvContent) {
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', filename);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
