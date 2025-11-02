import { useCallback, useEffect, useState } from 'react';
import Loading from '../../Components/Loading/Loading';
import CustomToast from '../../Components/CustomToast/CustomToast';
import instance from '../../axios';
import Pagination from '../../Components/Pagination/Pagination';
import { BiSearch, BiExport, BiX } from 'react-icons/bi'; // Thay X của Lucide bằng BiX cho nhất quán icon
import { FaFileInvoiceDollar } from 'react-icons/fa';
import { FiEye } from 'react-icons/fi';
import '../../App.css';

// --- Helper Functions (Không đổi) ---
const formatDate = (dateString) => {
  if (!dateString) return 'N/A';
  return new Date(dateString).toLocaleDateString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
  });
};

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

// --- Status Component (Không đổi) ---
const StatusBadge = ({ status }) => {
  let statusClass = '';
  let statusText = status;

  switch (status) {
    case 'Paid': statusClass = 'bg-success-soft'; statusText = 'Thành Công'; break;
    case 'Pending': statusClass = 'bg-warning-soft'; statusText = 'Chờ Thanh Toán'; break;
    default: statusClass = 'bg-secondary-soft'; statusText = 'Đã Hủy'; break;
  }
  return <span className={`badge rounded-pill fw-semibold ${statusClass}`}>{statusText}</span>;
};

// --- Định nghĩa trạng thái filter ban đầu ---
const initialFilters = {
  search: '',
  startDate: new Date(Date.now() - 6 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
  endDate: new Date().toISOString().split('T')[0],
};

const AdminRevenueReport = () => {
  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalItems, setTotalItems] = useState(0);
  const [modal, setModal] = useState({ show: false, detail: null });
  const [filters, setFilters] = useState(initialFilters);

  const totalPages = Math.ceil(totalItems / pageSize);

  const fetchInvoices = useCallback(async (page = 1, currentFilters = filters) => {
    setLoading(true);
    try {
      const params = {
        page,
        pageSize,
        search: currentFilters.search,
        startDate: currentFilters.startDate,
        endDate: currentFilters.endDate,
      };
      const response = await instance.get('Reports/GetDetailedRevenueReportAsync', { params });

      setInvoices(response?.content?.items || []);
      setTotalItems(response?.content?.totalItems || 0);
    } catch (error) {
      const errorMsg = error.response?.data?.message || error.message || "Lỗi không xác định";
      setToast({ type: 'error', message: `Lỗi khi tải hóa đơn: ${errorMsg}` });
    } finally {
      setLoading(false);
    }
  }, [pageSize, filters]);

  useEffect(() => {
    fetchInvoices(1);
  }, []); // Chỉ fetch 1 lần khi component mount

  const handleFilterAction = () => {
    if (new Date(filters.startDate) > new Date(filters.endDate)) {
      setToast({ type: 'error', message: 'Ngày bắt đầu không được lớn hơn ngày kết thúc' });
      return;
    }
    setCurrentPage(1);
    fetchInvoices(1, filters);
  };

  const clearFilters = useCallback(() => {
    setFilters(initialFilters);
    setCurrentPage(1);
    fetchInvoices(1, initialFilters);
  }, [fetchInvoices]);

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      e.preventDefault(); // Ngăn form submit mặc định nếu có
      handleFilterAction();
    }
  };

  const handlePageChange = (selectedPage) => {
    const newPage = selectedPage.selected + 1;
    setCurrentPage(newPage);
    fetchInvoices(newPage, filters);
  };

  const handleFilterChange = (field, value) => {
    setFilters(prev => ({ ...prev, [field]: value }));
  };

  const exportToCsv = () => {
    // Logic exportToCsv giữ nguyên
  };

  // Hàm renderModal không có gì thay đổi
  const renderModal = () => {
    if (!modal.show || !modal.detail) return null;
    const detail = modal.detail;
    return (
      <>
        <div className="modal-backdrop fade show"></div>
        <div className="modal fade show d-block" tabIndex="-1" onClick={() => setModal({ show: false, detail: null })}>
          <div className="modal-dialog modal-dialog-centered modal-lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-content border-0 shadow-lg">
              <div className="modal-header"><h5 className="modal-title d-flex align-items-center gap-2"><FaFileInvoiceDollar className="text-primary" />Hóa Đơn Chi Tiết</h5><button type="button" className="btn-close" onClick={() => setModal({ show: false, detail: null })}></button></div>
              <div className="modal-body">{/* Nội dung modal giữ nguyên */}</div>
            </div>
          </div>
        </div>
      </>
    );
  };

  return (
    <div className="d-flex">
      <main className="main-content flex-grow-1 p-4 d-flex flex-column gap-4">
        {toast && <CustomToast type={toast.type} message={toast.message} onClose={() => setToast(null)} />}

        <header className="d-flex justify-content-between align-items-center flex-shrink-0">
          <h1 className="h4 mb-0">Báo Cáo Doanh Thu</h1>
          <div className="d-flex gap-2">
            <button type="button" className="btn btn-outline-secondary d-flex align-items-center gap-2" onClick={exportToCsv}><BiExport /> Xuất CSV</button>
          </div>
        </header>

        <div className="card shadow-sm border-0 flex-shrink-0">
          <div className="card-body p-4">
            <form onSubmit={(e) => { e.preventDefault(); handleFilterAction(); }}>
              <div className="row g-3">
                {/* Hàng 1: Các ô nhập liệu */}
                <div className="col-md-5">
                  <label className="form-label small text-muted">Tìm theo tên bệnh nhân</label>
                  <div className="input-group">
                    <span className="input-group-text"><BiSearch /></span>
                    <input type="text" className="form-control" placeholder="Nhập tên và nhấn Enter..." value={filters.search} onChange={(e) => handleFilterChange('search', e.target.value)} onKeyDown={handleKeyDown} />
                  </div>
                </div>
                <div className="col-md-3">
                  <label htmlFor="startDate" className="form-label small text-muted">Từ ngày</label>
                  <input type="date" id="startDate" className="form-control" value={filters.startDate} onChange={(e) => handleFilterChange('startDate', e.target.value)} onKeyDown={handleKeyDown} />
                </div>
                <div className="col-md-3">
                  <label htmlFor="endDate" className="form-label small text-muted">Đến ngày</label>
                  <input type="date" id="endDate" className="form-control" value={filters.endDate} onChange={(e) => handleFilterChange('endDate', e.target.value)} onKeyDown={handleKeyDown} />
                </div>

                {/* Hàng 2: Các nút hành động */}
                <div className="col-md-12 d-flex justify-content-end gap-2 mt-3">
                  <button type="submit" className="btn btn-primary d-flex align-items-center gap-2" disabled={loading}><BiSearch /> Lọc Dữ Liệu</button>
                  <button type="button" className="btn btn-outline-danger d-flex align-items-center gap-2" onClick={clearFilters}><BiX size={20} /> Xóa bộ lọc</button>
                
                </div>
              </div>
            </form>
          </div>
        </div>

        <div className="card shadow-sm border-0 table-panel">
          {loading ? (<Loading isLoading={loading} />) : (
            <>
              <div className="table-responsive-container">
                <table className="table table-hover clinic-table mb-0">
                  {/* ... Nội dung bảng giữ nguyên ... */}
                  <thead className='p-4'><tr><th className="px-4">Mã HĐ</th><th>Ngày Lập</th><th>Bệnh Nhân</th><th className='text-end'>Tổng Cộng</th><th>Ngày Hẹn</th><th className='text-center'>Trạng Thái</th><th className="text-center px-4">Chi Tiết</th></tr></thead>
                  <tbody>{invoices.length === 0 ? (<tr><td colSpan="7" className="text-center p-5 text-muted">Không có dữ liệu</td></tr>) : (invoices.map(item => (<tr key={item.invoiceId}><td className="px-4"><span className='invoice-id'>{`#${item.invoiceId}`}</span></td><td>{formatDate(item.invoiceDate)}</td><td>{item.patientName}</td><td className="text-end fw-semibold">{formatCurrency(item.totalAmount)}</td><td>{formatDate(item.appointmentDate)}</td><td className='text-center'><StatusBadge status={item.status} /></td><td className="text-center px-4"><button className="btn btn-light btn-lg" title="Xem chi tiết" onClick={() => setModal({ show: true, detail: item })}><FiEye /></button></td></tr>)))}</tbody>
                </table>
              </div>
              {totalPages > 1 && (
                <div className="card-footer p-3 border-0 flex-shrink-0">
                  <Pagination
                    pageCount={totalPages}
                    currentPage={currentPage - 1}
                    onPageChange={handlePageChange}
                    isLoading={loading}
                  />
                </div>
              )}
            </>
          )}
        </div>

        {renderModal()}
      </main>
    </div>
  );
};

export default AdminRevenueReport;