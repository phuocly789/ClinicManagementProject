import React, { useState, useEffect, useCallback, useRef, memo } from 'react';
import { useDropzone } from 'react-dropzone';
import * as XLSX from 'xlsx';
import Pagination from '../../Components/Pagination/Pagination';
import ConfirmDeleteModal from '../../Components/CustomToast/DeleteConfirmModal';
import CustomToast from '../../Components/CustomToast/CustomToast';
import { Pencil, Trash, Download, Upload, FileSpreadsheet, CheckCircle, XCircle, Search, X, PlusCircle } from 'lucide-react';
import Loading from '../../Components/Loading/Loading';
import instance from '../../axios';
import '../../App.css';

// --- Hằng số và Regex (Không đổi) ---
const medicineTypes = [
  "Giảm đau, hạ sốt",
  "Kháng sinh",
  "Kháng dị ứng",
  "Dạ dày - tiêu hóa",
  "Tiểu đường",
  "Huyết áp - tim mạch",
  "Hô hấp",
  "Da liễu",
  "Mỡ máu",
  "Mắt - tai - mũi - họng",
  "Thần kinh - giấc ngủ",
  "Cơ - xương - khớp",
  "Nội tiết - hormone",
  "Vitamin - khoáng chất",
  "Thuốc gây tê - gây mê",
  "Chống đông - tim mạch",
  "Giải độc - cấp cứu",
  "Sát khuẩn - khử trùng",
  "Thuốc tiêm chuyên khoa",
  "Dinh dưỡng & thực phẩm y tế"
];

const units = [
  "Viên",
  "Viên nén",
  "Viên nang",
  "Viên sủi",
  "Ống tiêm",
  "Chai",
  "Lọ",
  "Gói",
  "Vỉ",
  "Tuýp",
  "Hít",
  "Bột",
  "Dung dịch",
  "Hỗn dịch",
  "Si-rô",
  "Thuốc nhỏ mắt",
  "Thuốc nhỏ mũi",
  "Xịt mũi",
  "Miếng dán",
  "Bơm tiêm định liều"
]


// --- Helper Functions & Components (Không đổi) ---
const formatVND = (amount) => {
  if (amount === null || amount === undefined) return 'N/A';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

const StockBadge = ({ quantity }) => {
  let statusClass = '';
  if (quantity < 100) statusClass = 'bg-danger-soft';
  else if (quantity < 500) statusClass = 'bg-warning-soft';
  else statusClass = 'bg-success-soft';
  return <span className={`badge rounded-pill fw-semibold ${statusClass}`}>{quantity}</span>;
};

// --- Giao diện Danh sách thuốc ---
const MedicineList = memo(({ medicines, isLoading, handleShowDeleteModal, handleShowEditForm, pageCount, currentPage, handlePageChange, onDownloadTemplate, onShowExportModal, onShowImport, applyFilters, clearFilters, filters, setFilters }) => {

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      applyFilters();
    }
  };

  return (
    <>
      <header className="d-flex justify-content-between align-items-center flex-shrink-0">
        <h1 className="h4 mb-0">Danh Sách Thuốc</h1>
        <div className="d-flex gap-2">
          <button className="btn btn-outline-secondary d-flex align-items-center gap-2" onClick={onDownloadTemplate}><Download size={16} /> Tải Template</button>
          <button className="btn btn-outline-secondary d-flex align-items-center gap-2" onClick={onShowExportModal}><FileSpreadsheet size={16} /> Export</button>
          <button className="btn btn-outline-secondary d-flex align-items-center gap-2" onClick={onShowImport}><Upload size={16} /> Import</button>
          <button className="btn btn-primary d-flex align-items-center gap-2" onClick={() => handleShowEditForm(null)}><PlusCircle size={16} /> Thêm Thuốc</button>
        </div>
      </header>

      <div className="card shadow-sm border-0 flex-shrink-0">
        <div className="card-body p-4">
          <form onSubmit={(e) => { e.preventDefault(); applyFilters(); }}>
            <div className="row g-3 align-items-end">
              <div className="col-md-4"><label className="form-label small text-muted">Tìm theo tên thuốc</label><div className="input-group"><span className="input-group-text"><Search size={16} /></span><input type="text" className="form-control" placeholder="Nhập tên và nhấn Enter..." value={filters.search || ''} onChange={(e) => setFilters(prev => ({ ...prev, search: e.target.value }))} onKeyDown={handleKeyDown} /></div></div>
              <div className="col-md-2"><label className="form-label small text-muted">Loại thuốc</label><select className="form-select" value={filters.type || ''} onChange={(e) => setFilters(prev => ({ ...prev, type: e.target.value }))} onKeyDown={handleKeyDown}><option value="">Tất cả</option>{medicineTypes.map(t => <option key={t} value={t}>{t}</option>)}</select></div>
              <div className="col-md-2"><label className="form-label small text-muted">Đơn vị</label><select className="form-select" value={filters.unit || ''} onChange={(e) => setFilters(prev => ({ ...prev, unit: e.target.value }))} onKeyDown={handleKeyDown}><option value="">Tất cả</option>{units.map(u => <option key={u} value={u}>{u}</option>)}</select></div>
              <div className="col-md-4"><label className="form-label small text-muted">Khoảng giá</label><div className="input-group"><input type="number" className="form-control" placeholder="Giá từ" value={filters.minPrice || ''} onChange={(e) => setFilters(prev => ({ ...prev, minPrice: e.target.value }))} onKeyDown={handleKeyDown} /><span className="input-group-text">→</span><input type="number" className="form-control" placeholder="Giá đến" value={filters.maxPrice || ''} onChange={(e) => setFilters(prev => ({ ...prev, maxPrice: e.target.value }))} onKeyDown={handleKeyDown} /></div></div>
              <div className="col-md-12 d-flex justify-content-between align-items-center mt-3">
                <div className="form-check form-switch"><input className="form-check-input" type="checkbox" role="switch" id="lowStockSwitch" checked={filters.lowStock || false} onChange={(e) => setFilters(prev => ({ ...prev, lowStock: e.target.checked }))} /><label className="form-check-label" htmlFor="lowStockSwitch">Chỉ hiển thị thuốc tồn kho thấp</label></div>
                <div className="d-flex gap-2"><button type="submit" className="btn btn-primary d-flex align-items-center gap-2"><Search size={16} /> Lọc</button><button type="button" className="btn btn-outline-danger d-flex align-items-center gap-2" onClick={clearFilters}><X size={16} /> Xóa bộ lọc</button></div>
              </div>
            </div>
          </form>
        </div>
      </div>

      <div className="card shadow-sm border-0 table-panel">
        {isLoading ? (<Loading isLoading={isLoading} />) : (
          <>
            <div className="table-responsive-container">
              <table className="table table-hover clinic-table mb-0">
                <thead><tr><th className="px-4">Mã</th><th>Tên Thuốc</th><th>Loại</th><th>ĐV</th><th className="text-end">Giá</th><th className="text-center">Tồn Kho</th><th>Mô Tả</th><th className="text-center px-4">Hành Động</th></tr></thead>
                <tbody>
                  {medicines.length === 0 ? (<tr><td colSpan="8" className="text-center p-5 text-muted">Không có dữ liệu</td></tr>) : (medicines.map((m) => (<tr key={m.medicineId}><td className="px-4"><span className="user-id">{m.medicineId}</span></td><td>{m.medicineName}</td><td>{m.medicineType}</td><td>{m.unit}</td><td className="text-end fw-semibold">{formatVND(m.price)}</td><td className="text-center"><StockBadge quantity={m.stockQuantity} /></td><td title={m.description}>{m.description?.length > 100 ? m.description.substring(0, 100) + '...' : m.description || '—'}</td><td className="text-center px-4"><button className="btn btn-light btn-sm me-2" title="Sửa" onClick={() => handleShowEditForm(m)}><Pencil size={16} /></button><button className="btn btn-light btn-sm text-danger" title="Xóa" onClick={() => handleShowDeleteModal(m.medicineId)}><Trash size={16} /></button></td></tr>)))}
                </tbody>
              </table>
            </div>
            {pageCount > 1 && (<div className="card-footer p-3 border-0 flex-shrink-0"><Pagination pageCount={pageCount} currentPage={currentPage} onPageChange={handlePageChange} isLoading={isLoading} /></div>)}
          </>
        )}
      </div>
    </>
  );
});

// --- Modal Form Thêm/Sửa ---
const MedicineFormModal = memo(({ show, onHide, isEditMode, medicine, onSubmit, isLoading }) => {
  const [errors, setErrors] = useState({});
  const formRef = useRef(null);

  useEffect(() => {
    // Xóa lỗi khi modal được mở
    if (show) {
      setErrors({});
    }
  }, [show]);

  const handleSubmit = (e) => {
    e.preventDefault();
    // Logic validate có thể thêm ở đây nếu cần
    const formData = new FormData(formRef.current);
    onSubmit(formData);
  };

  if (!show) return null;

  return (
    <>
      <div className="modal-backdrop fade show"></div>
      <div className="modal fade show d-block" tabIndex="-1" onClick={onHide}>
        <div className="modal-dialog modal-dialog-centered modal-lg" onClick={(e) => e.stopPropagation()}>
          <div className="modal-content">
            <form ref={formRef} onSubmit={handleSubmit}>
              <div className="modal-header">
                <h5 className="modal-title">{isEditMode ? 'Chỉnh Sửa Thuốc' : 'Thêm Thuốc Mới'}</h5>
                <button type="button" className="btn-close" onClick={onHide}></button>
              </div>
              <div className="modal-body">
                <div className="row">
                  <div className="col-md-6 mb-3"><label className="form-label">Tên Thuốc</label><input type="text" name="medicineName" defaultValue={isEditMode ? medicine?.medicineName : ''} className={`form-control ${errors.medicineName ? 'is-invalid' : ''}`} placeholder="Nhập tên thuốc" required /><div className="invalid-feedback">{errors.medicineName}</div></div>
                  <div className="col-md-6 mb-3"><label className="form-label">Loại Thuốc</label><select name="medicineType" defaultValue={isEditMode ? medicine?.medicineType : ''} className={`form-select ${errors.medicineType ? 'is-invalid' : ''}`} required><option value="" disabled>Chọn loại thuốc</option>{medicineTypes.map(type => <option key={type} value={type}>{type}</option>)}</select><div className="invalid-feedback">{errors.medicineType}</div></div>
                </div>
                <div className="row">
                  <div className="col-md-6 mb-3"><label className="form-label">Đơn Vị</label><select name="unit" defaultValue={isEditMode ? medicine?.unit : ''} className={`form-select ${errors.unit ? 'is-invalid' : ''}`} required><option value="" disabled>Chọn đơn vị</option>{units.map(unit => <option key={unit} value={unit}>{unit}</option>)}</select><div className="invalid-feedback">{errors.unit}</div></div>
                  <div className="col-md-6 mb-3"><label className="form-label">Giá Bán</label><input type="number" name="price" defaultValue={isEditMode ? medicine?.price : ''} step="100" className={`form-control ${errors.price ? 'is-invalid' : ''}`} placeholder="Nhập giá bán" required /><div className="invalid-feedback">{errors.price}</div></div>
                </div>
                <div className="row">
                  <div className="col-md-6 mb-3"><label className="form-label">Tồn Kho</label><input type="number" name="stockQuantity" defaultValue={isEditMode ? medicine?.stockQuantity : ''} className={`form-control ${errors.stockQuantity ? 'is-invalid' : ''}`} placeholder="Nhập số lượng tồn kho" required /><div className="invalid-feedback">{errors.stockQuantity}</div></div>
                  <div className="col-md-6 mb-3"><label className="form-label">Mô Tả</label><textarea name="description" defaultValue={isEditMode ? medicine?.description : ''} className={`form-control ${errors.description ? 'is-invalid' : ''}`} placeholder="Nhập mô tả" rows="3" ></textarea><div className="invalid-feedback">{errors.description}</div></div>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={onHide}>Hủy</button>
                <button type="submit" className="btn btn-primary" disabled={isLoading}>{isLoading ? 'Đang lưu...' : isEditMode ? 'Lưu Thay Đổi' : 'Thêm Mới'}</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </>
  );
});

// --- Component Chính ---
const AdminMedicine = () => {
  const [medicines, setMedicines] = useState([]);
  const [currentPage, setCurrentPage] = useState(0);
  const [pageCount, setPageCount] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [medicineToDelete, setMedicineToDelete] = useState(null);
  const [toast, setToast] = useState(null);
  const [editMedicine, setEditMedicine] = useState(null);
  const [showFormModal, setShowFormModal] = useState(false);
  const [filters, setFilters] = useState({
    search: '',
    type: '',
    unit: '',
    minPrice: '',
    maxPrice: '',
    lowStock: false,
  });

  const showToast = useCallback((type, message) => { setToast({ type, message }); }, []);
  const hideToast = useCallback(() => { setToast(null); }, []);

  const fetchMedicines = useCallback(async (page = 1, currentFilters = filters) => {
    setIsLoading(true);
    try {
      const params = {
        page: page,
        pageSize: 10,
        search: currentFilters.search || null,
        type: currentFilters.type || null,
        unit: currentFilters.unit || null,
        minPrice: currentFilters.minPrice || null,
        maxPrice: currentFilters.maxPrice || null,
        lowStock: currentFilters.lowStock || false,
      };

      const response = await instance.get('Medicine/GetAllMedicinesAsync', { params });

      if (response && response.content) {
        setMedicines(response.content.items || []);
        setPageCount(Math.ceil(response.content.totalItems / response.content.pageSize));
        setCurrentPage(response.content.page - 1);
      } else {
        setMedicines([]); setPageCount(0); setCurrentPage(0);
      }
    } catch (error) {
      const errorMsg = error.response?.data?.message || error.message || "Lỗi không xác định";
      showToast('error', `Lỗi khi tải dữ liệu: ${errorMsg}`);
    } finally {
      setIsLoading(false);
    }
  }, [showToast, filters]);

  useEffect(() => {
    fetchMedicines(1, filters);
  }, []); // Chỉ fetch lần đầu khi component được mount

  const applyFilters = () => {
    setCurrentPage(0); // Reset về trang đầu khi lọc
    fetchMedicines(1, filters);
  };

  const clearFilters = useCallback(() => {
    const resetFilters = { search: '', type: '', unit: '', minPrice: '', maxPrice: '', lowStock: false };
    setFilters(resetFilters);
    setCurrentPage(0);
    fetchMedicines(1, resetFilters);
  }, [fetchMedicines]);


  const handlePageChange = useCallback(({ selected }) => {
    const newPage = selected + 1;
    setCurrentPage(selected);
    fetchMedicines(newPage, filters);
  }, [fetchMedicines, filters]);

  const handleDelete = useCallback(async (medicineId) => {
    setIsLoading(true);
    try {
      await instance.delete(`Medicine/DeleteMedicineAsync/${medicineId}`);
      showToast('success', 'Xóa thuốc thành công');
      fetchMedicines(medicines.length === 1 && currentPage > 0 ? currentPage : currentPage + 1, filters);
    } catch (error) {
      const errorMsg = error.response?.data?.message || "Lỗi khi xóa thuốc.";
      showToast('error', errorMsg);
    } finally {
      setIsLoading(false);
      setShowDeleteModal(false);
      setMedicineToDelete(null);
    }
  }, [currentPage, fetchMedicines, showToast, medicines.length, filters]);

  const handleFormSubmit = useCallback(async (formData) => {
    const isEdit = !!editMedicine;
    const data = {
      medicineName: formData.get('medicineName'),
      medicineType: formData.get('medicineType'),
      unit: formData.get('unit'),
      price: parseFloat(formData.get('price')),
      stockQuantity: parseInt(formData.get('stockQuantity')),
      description: formData.get('description') || '',
    };

    setIsLoading(true);
    try {
      const response = isEdit
        ? await instance.put(`Medicine/UpdateMedicineAsync/${editMedicine.medicineId}`, data)
        : await instance.post('Medicine/CreateMedicineAsync', data);

      showToast('success', response.message || (isEdit ? 'Cập nhật thành công' : 'Thêm mới thành công'));
      setShowFormModal(false);
      setEditMedicine(null);
      fetchMedicines(isEdit ? currentPage + 1 : 1, filters); // Tải lại trang hiện tại hoặc trang đầu
    } catch (error) {
      const errorMsg = error.response?.data?.message || "Đã xảy ra lỗi.";
      showToast('error', errorMsg);
    } finally {
      setIsLoading(false);
    }
  }, [editMedicine, showToast, fetchMedicines, currentPage, filters]);


  const handleShowDeleteModal = useCallback((medicineId) => { setMedicineToDelete(medicineId); setShowDeleteModal(true); }, []);
  const handleCancelDelete = useCallback(() => { setShowDeleteModal(false); setMedicineToDelete(null); }, []);
  const handleShowFormModal = useCallback((medicine) => { setEditMedicine(medicine); setShowFormModal(true); }, []);
  const handleCloseFormModal = useCallback(() => { setEditMedicine(null); setShowFormModal(false); }, []);

  // NOTE: Chức năng Import/Export cần được xây dựng ở backend.
  const handleDownloadTemplate = () => showToast('info', 'Chức năng đang được phát triển.');
  const onShowExportModal = () => showToast('info', 'Chức năng đang được phát triển.');
  const onShowImport = () => showToast('info', 'Chức năng đang được phát triển.');

  return (
    <div className="d-flex">
      <main className="main-content flex-grow-1 p-4 d-flex flex-column gap-4">
        {toast && <CustomToast type={toast.type} message={toast.message} onClose={hideToast} />}

        <MedicineList
          medicines={medicines} isLoading={isLoading}
          handleShowDeleteModal={handleShowDeleteModal} handleShowEditForm={handleShowFormModal}
          pageCount={pageCount} currentPage={currentPage} handlePageChange={handlePageChange}
          onDownloadTemplate={handleDownloadTemplate} onShowExportModal={onShowExportModal} onShowImport={onShowImport}
          applyFilters={applyFilters} clearFilters={clearFilters}
          filters={filters} setFilters={setFilters}
        />

        <MedicineFormModal
          show={showFormModal}
          onHide={handleCloseFormModal}
          isEditMode={!!editMedicine}
          medicine={editMedicine}
          onSubmit={handleFormSubmit}
          isLoading={isLoading}
        />

        <ConfirmDeleteModal
          isOpen={showDeleteModal}
          title="Xác nhận xóa"
          message="Bạn có chắc chắn muốn xóa thuốc này?"
          onConfirm={() => handleDelete(medicineToDelete)}
          onCancel={handleCancelDelete}
        />
      </main>
    </div>
  );
};

export default AdminMedicine;