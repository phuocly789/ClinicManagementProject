import React, { useState, useEffect, useCallback, useRef, memo } from 'react';
import Pagination from '../../Components/Pagination/Pagination';
import ConfirmDeleteModal from '../../Components/CustomToast/DeleteConfirmModal';
import CustomToast from '../../Components/CustomToast/CustomToast';
import { Eye, Pencil, Trash, Search, DollarSign, Calendar, PlusCircle, X, Printer, Plus, Trash2 } from 'lucide-react';
import Loading from '../../Components/Loading/Loading';
import instance from '../../axios';
import '../../App.css';

// --- Helper Functions & Components ---
const formatVND = (value) => {
  if (value === null || value === undefined) return 'N/A';
  return Number(value).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
};

// --- COMPONENT: Giao diện Danh sách ---
const InventoryList = memo(({ inventories, isLoading, suppliers, handleShowDeleteModal, handleShowEditForm, handleShowDetail, pageCount, currentPage, handlePageChange, filters, setFilters, applyFilters, clearFilters }) => {
  const handleKeyDown = (e) => { if (e.key === 'Enter') { e.preventDefault(); applyFilters(); } };

  return (
    <>
      <header className="d-flex justify-content-between align-items-center flex-shrink-0">
        <h1 className="h4 mb-0">Quản Lý Nhập Kho</h1>
        <button className="btn btn-primary d-flex align-items-center gap-2" onClick={() => handleShowEditForm(null)}><PlusCircle size={16} /> Thêm Phiếu Nhập</button>
      </header>
      <div className="card shadow-sm border-0 flex-shrink-0">
        <div className="card-body p-4">
          <form onSubmit={(e) => { e.preventDefault(); applyFilters(); }}>
            <div className="row g-3">
              <div className="col-md-4"><label className="form-label small text-muted">Tìm theo nhà cung cấp</label><div className="input-group"><span className="input-group-text"><Search /></span><input type="text" className="form-control" placeholder="Nhập tên NCC và nhấn Enter..." value={filters.search} onChange={(e) => setFilters(p => ({ ...p, search: e.target.value }))} onKeyDown={handleKeyDown} /></div></div>
              <div className="col-md-4"><label className="form-label small text-muted">Lọc theo nhà cung cấp</label><select className="form-select" value={filters.supplierId} onChange={(e) => setFilters(p => ({ ...p, supplierId: e.target.value }))} onKeyDown={handleKeyDown}><option value="">Tất cả nhà cung cấp</option>{suppliers.map((s) => (<option key={s.supplierId} value={s.supplierId}>{s.supplierName}</option>))}</select></div>
              <div className="col-md-4"><label className="form-label small text-muted">Tổng tiền</label><div className="input-group"><span className="input-group-text"><DollarSign size={16} /></span><input type="number" className="form-control" placeholder="Từ" value={filters.minTotal} onChange={(e) => setFilters(p => ({ ...p, minTotal: e.target.value }))} onKeyDown={handleKeyDown} /><span className="input-group-text">→</span><input type="number" className="form-control" placeholder="Đến" value={filters.maxTotal} onChange={(e) => setFilters(p => ({ ...p, maxTotal: e.target.value }))} onKeyDown={handleKeyDown} /></div></div>
              <div className="col-md-4"><label className="form-label small text-muted">Ngày nhập từ</label><div className="input-group"><span className="input-group-text"><Calendar size={16} /></span><input type="date" className="form-control" value={filters.startDate} onChange={(e) => setFilters(p => ({ ...p, startDate: e.target.value }))} onKeyDown={handleKeyDown} /></div></div>
              <div className="col-md-4"><label className="form-label small text-muted">Ngày nhập đến</label><div className="input-group"><span className="input-group-text"><Calendar size={16} /></span><input type="date" className="form-control" value={filters.endDate} onChange={(e) => setFilters(p => ({ ...p, endDate: e.target.value }))} onKeyDown={handleKeyDown} /></div></div>
              <div className="col-md-4 d-flex align-items-end justify-content-end gap-2"><button type="submit" className="btn btn-primary d-flex align-items-center gap-2"><Search /> Lọc</button><button type="button" className="btn btn-outline-danger d-flex align-items-center gap-2" onClick={clearFilters}><X /> Xóa Lọc</button></div>
            </div>
          </form>
        </div>
      </div>
      <div className="card shadow-sm border-0 table-panel">
        {isLoading && inventories.length === 0 ? (<Loading isLoading={isLoading} />) : (
          <>
            <div className="table-responsive-container">
              <table className="table table-hover clinic-table mb-0">
                <thead><tr><th className="px-4">Mã Phiếu</th><th>Nhà Cung Cấp</th><th>Ngày Nhập</th><th className="text-end">Tổng Tiền</th><th>Ghi Chú</th><th className="text-center px-4">Hành Động</th></tr></thead>
                <tbody>
                  {!isLoading && inventories.length === 0 ? (<tr><td colSpan="6" className="text-center p-5 text-muted">Không có dữ liệu</td></tr>) : (inventories.map((inv) => (<tr key={inv.importId} style={{ cursor: 'pointer' }} onClick={() => handleShowDetail(inv)}><td className="px-4"><span className="user-id">#{inv.importId}</span></td><td>{inv.supplierName || 'N/A'}</td><td>{new Date(inv.importDate).toLocaleDateString('vi-VN')}</td><td className="text-end fw-semibold">{formatVND(inv.totalAmount)}</td><td title={inv.notes}>{inv.notes?.length > 30 ? inv.notes.substring(0, 30) + '...' : inv.notes || '—'}</td><td className="text-center px-4" onClick={(e) => e.stopPropagation()}><button className="btn btn-light btn-sm me-2" title="Xem chi tiết" onClick={() => handleShowDetail(inv)}><Eye size={16} /></button><button className="btn btn-light btn-sm me-2" title="Sửa" onClick={() => handleShowEditForm(inv)}><Pencil size={16} /></button><button className="btn btn-light btn-sm text-danger" title="Xóa" onClick={() => handleShowDeleteModal(inv.importId)}><Trash size={16} /></button></td></tr>)))}
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

// --- COMPONENT: Modal Form Thêm/Sửa ---
const InventoryFormModal = memo(({ show, onHide, isEditMode, inventory, onSubmit, isLoading, suppliers, medicines }) => {
  const [items, setItems] = useState([]);
  const formRef = useRef(null);

  useEffect(() => {
    if (show) {
      if (isEditMode && inventory?.details) {
        setItems(inventory.details.map(d => ({ medicineId: d.medicineId, quantity: d.quantity, importPrice: d.importPrice })));
      } else {
        setItems([{ medicineId: '', quantity: 1, importPrice: '' }]);
      }
    }
  }, [show, isEditMode, inventory]);

  const handleItemChange = (index, field, value) => { setItems(prev => { const newItems = [...prev]; newItems[index][field] = value; return newItems; }); };
  const handleAddItem = () => { setItems([...items, { medicineId: '', quantity: 1, importPrice: '' }]); };
  const handleRemoveItem = (index) => { if (items.length > 1) { setItems(items.filter((_, i) => i !== index)); } };

  const handleSubmit = (e) => { e.preventDefault(); const formData = new FormData(formRef.current); onSubmit(formData, items); };

  if (!show) return null;

  const totalAmount = items.reduce((sum, item) => sum + (Number(item.quantity || 0) * Number(item.importPrice || 0)), 0);

  return (
    <><div className="modal-backdrop fade show"></div>
      <div className="modal fade show d-block" tabIndex="-1" onClick={onHide}>
        <div className="modal-dialog modal-dialog-centered modal-xl" onClick={(e) => e.stopPropagation()}>
          <form ref={formRef} onSubmit={handleSubmit} className="modal-content">
            <div className="modal-header"><h5 className="modal-title">{isEditMode ? `Sửa Phiếu Nhập #${inventory.importId}` : 'Tạo Phiếu Nhập Kho Mới'}</h5><button type="button" className="btn-close" onClick={onHide}></button></div>
            <div className="modal-body">
              <div className="row g-3">
                <div className="col-md-6"><label className="form-label">Nhà Cung Cấp</label><select name="supplierId" defaultValue={inventory?.supplierId || ''} className="form-select" required><option value="" disabled>Chọn nhà cung cấp</option>{suppliers.map(s => <option key={s.supplierId} value={s.supplierId}>{s.supplierName}</option>)}</select></div>
                <div className="col-md-6"><label className="form-label">Ngày Nhập</label><input type="date" name="importDate" defaultValue={(inventory?.importDate || new Date().toISOString()).split('T')[0]} className="form-control" required /></div>
                <div className="col-12"><label className="form-label">Ghi Chú</label><textarea name="notes" defaultValue={inventory?.notes || ''} className="form-control" rows="2" placeholder="Thông tin thêm..."></textarea></div>
              </div><hr /><h6>Chi tiết phiếu nhập</h6>
              {items.map((item, index) => (
                <div key={index} className="row g-2 align-items-center mb-2">
                  <div className="col-md-5"><select className="form-select" value={item.medicineId} onChange={(e) => handleItemChange(index, 'medicineId', e.target.value)} required><option value="" disabled>Chọn thuốc</option>{medicines.map(m => <option key={m.medicineId} value={m.medicineId}>{m.medicineName} ({m.unit})</option>)}</select></div>
                  <div className="col-md-3"><input type="number" className="form-control" placeholder="Số lượng" value={item.quantity} onChange={(e) => handleItemChange(index, 'quantity', e.target.value)} min="1" required /></div>
                  <div className="col-md-3"><input type="number" className="form-control" placeholder="Giá nhập" value={item.importPrice} onChange={(e) => handleItemChange(index, 'importPrice', e.target.value)} min="0" required /></div>
                  <div className="col-md-1"><button type="button" className="btn btn-outline-danger btn-sm" onClick={() => handleRemoveItem(index)} disabled={items.length <= 1}><Trash2 size={16} /></button></div>
                </div>
              ))}
              <button type="button" className="btn btn-success btn-sm mt-2 d-flex align-items-center gap-2" onClick={handleAddItem}><Plus size={16} /> Thêm Thuốc</button>
              <div className="text-end mt-3 fs-5"><strong>Tổng cộng: <span className="text-danger">{formatVND(totalAmount)}</span></strong></div>
            </div>
            <div className="modal-footer"><button type="button" className="btn btn-secondary" onClick={onHide}>Hủy</button><button type="submit" className="btn btn-primary" disabled={isLoading}>{isLoading ? 'Đang lưu...' : 'Lưu Phiếu Nhập'}</button></div>
          </form>
        </div>
      </div></>
  );
});

// --- COMPONENT: Modal Chi tiết ---
const InventoryDetailModal = memo(({ show, onHide, inventory, isLoading }) => {
  const printableAreaRef = useRef(null);
  const handlePrint = () => { /* Logic in đã có ở trên */ };
  if (!show) return null;
  return (
    <><div className="modal-backdrop fade show"></div>
      <div className="modal fade show d-block" tabIndex="-1" onClick={onHide}>
        <div className="modal-dialog modal-dialog-centered modal-lg" onClick={(e) => e.stopPropagation()}>
          <div className="modal-content">
            <div className="modal-header"><h5 className="modal-title">Chi Tiết Phiếu Nhập Kho #{inventory?.importId}</h5><button type="button" className="btn-close" onClick={onHide}></button></div>
            <div className="modal-body" ref={printableAreaRef}>
              {isLoading || !inventory ? (<div className="text-center p-5"><div className="spinner-border text-primary"></div></div>) : (
                <><div className="bg-light p-3 rounded border mb-4"><div className="row"><div className="col-md-6"><small className="text-muted d-block">Nhà cung cấp</small><span className="fw-semibold">{inventory.supplierName}</span></div><div className="col-md-3"><small className="text-muted d-block">Ngày nhập</small><span className="fw-semibold">{new Date(inventory.importDate).toLocaleDateString('vi-VN')}</span></div><div className="col-md-3"><small className="text-muted d-block">Tổng tiền</small><span className="fw-bold text-danger">{formatVND(inventory.totalAmount)}</span></div>{inventory.notes && <div className="col-12 mt-2"><small className="text-muted d-block">Ghi chú</small><span>{inventory.notes}</span></div>}</div></div><h6>Chi tiết thuốc đã nhập</h6><table className='table table-sm table-bordered'><thead><tr><th>Tên Thuốc</th><th className='text-center'>Số Lượng</th><th className='text-end'>Giá Nhập</th><th className='text-end'>Thành Tiền</th></tr></thead><tbody>{inventory.details?.map((d, index) => (<tr key={index}><td><div className="fw-semibold">{d.medicineName || 'N/A'}</div></td><td className='text-center align-middle'>{d.quantity}</td><td className='text-end align-middle'>{formatVND(d.importPrice)}</td><td className='text-end align-middle fw-semibold'>{formatVND(d.quantity * d.importPrice)}</td></tr>))}</tbody></table></>
              )}
            </div>
            <div className="modal-footer"><button type="button" className="btn btn-secondary" onClick={onHide}>Đóng</button><button type="button" className="btn btn-primary d-flex align-items-center gap-2" onClick={handlePrint}><Printer size={16} /> In Phiếu</button></div>
          </div>
        </div>
      </div></>
  );
});


// --- COMPONENT CHÍNH: AdminInventory ---
const initialFilters = { search: '', supplierId: '', startDate: '', endDate: '', minTotal: '', maxTotal: '' };

const AdminInventory = () => {
  const [inventories, setInventories] = useState([]);
  const [suppliers, setSuppliers] = useState([]);
  const [medicines, setMedicines] = useState([]);
  const [currentPage, setCurrentPage] = useState(0);
  const [pageCount, setPageCount] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [toast, setToast] = useState(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [inventoryToDelete, setInventoryToDelete] = useState(null);
  const [showFormModal, setShowFormModal] = useState(false);
  const [editInventory, setEditInventory] = useState(null);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [selectedInventory, setSelectedInventory] = useState(null);
  const [filters, setFilters] = useState(initialFilters);

  const showToast = useCallback((type, message) => { setToast({ type, message }); }, []);
  const hideToast = useCallback(() => { setToast(null); }, []);

  const fetchCommonData = useCallback(async () => {
    if (suppliers.length > 0 && medicines.length > 0) return;
    try {
      const [suppliersRes, medicinesRes] = await Promise.all([
        instance.get('Supplier/GetAllSupplierAsync', { params: { pageSize: 1000 } }),
        instance.get('Medicine/GetAllMedicinesAsync', { params: { pageSize: 1000 } })
      ]);
      setSuppliers(suppliersRes.content?.items || []);
      setMedicines(medicinesRes.content?.items || []);
    } catch (error) { showToast('error', 'Lỗi khi tải dữ liệu chung.'); }
  }, [suppliers.length, medicines.length, showToast]);

  const fetchInventories = useCallback(async (page = 1, currentFilters = filters) => {
    setIsLoading(true);
    try {
      const params = {
        page, pageSize: 10,
        search: currentFilters.search || null,
        supplierId: currentFilters.supplierId || null,
        startDate: currentFilters.startDate || null,
        endDate: currentFilters.endDate || null,
        minTotal: currentFilters.minTotal || null,
        maxTotal: currentFilters.maxTotal || null,
      };
      const response = await instance.get('Import/GetAllImportBillsAsync', { params });
      if (response?.content) {
        setInventories(response.content.items || []);
        setPageCount(Math.ceil(response.content.totalItems / response.content.pageSize));
        setCurrentPage(response.content.page - 1);
      }
    } catch (error) { showToast('error', `Lỗi tải phiếu nhập: ${error.response?.data?.message || error.message}`); } finally { setIsLoading(false); }
  }, [showToast, filters]);

  useEffect(() => { fetchInventories(1); fetchCommonData(); }, []);

  const applyFilters = () => { setCurrentPage(0); fetchInventories(1, filters); };
  const clearFilters = useCallback(() => { setFilters(initialFilters); setCurrentPage(0); fetchInventories(1, initialFilters); }, [fetchInventories]);
  const handlePageChange = useCallback(({ selected }) => { fetchInventories(selected + 1, filters); }, [fetchInventories, filters]);
  const handleShowDeleteModal = (id) => { setInventoryToDelete(id); setShowDeleteModal(true); };
  const handleCancelDelete = () => { setInventoryToDelete(null); setShowDeleteModal(false); };

  const handleDelete = useCallback(async () => {
    showToast('info', 'Chức năng xóa đang được phát triển.');
    // TODO: Mở comment khi có API DELETE
    // try {
    //     await instance.delete(`Import/DeleteImportBillAsync/${inventoryToDelete}`);
    //     showToast('success', `Đã xóa phiếu nhập #${inventoryToDelete}`);
    //     fetchInventories(1, filters);
    // } catch (error) { showToast('error', `Lỗi khi xóa`); }
    setShowDeleteModal(false);
  }, [showToast]);

  const handleShowEditForm = async (inventory) => {
    await fetchCommonData(); // Đảm bảo có NCC và thuốc trước khi mở modal
    if (inventory) {
      // Fetch chi tiết để có mục `details` cho form sửa
      setIsLoading(true);
      try {
        const res = await instance.get(`Import/GetImportBillByIdAsync/${inventory.importId}`);
        setEditInventory(res.content);
      } catch {
        showToast('error', 'Không thể tải dữ liệu để sửa.');
        return;
      } finally {
        setIsLoading(false);
      }
    } else {
      setEditInventory(null);
    }
    setShowFormModal(true);
  };

  const handleShowDetail = async (inventory) => {
    setIsLoading(true);
    try {
      const response = await instance.get(`Import/GetImportBillByIdAsync/${inventory.importId}`);
      setSelectedInventory(response.content);
      console.log("Phiếu đã chọn", selectedInventory);
      
      setShowDetailModal(true);
    } catch (error) { showToast('error', `Không thể tải chi tiết phiếu nhập.`); } finally { setIsLoading(false); }
  };

  const handleFormSubmit = useCallback(async (formData, items) => {
    const isEdit = !!editInventory;
    const data = {
      supplierId: parseInt(formData.get('supplierId')),
      importDate: formData.get('importDate'),
      notes: formData.get('notes'),
      // createdBy: 1, // Lấy ID người dùng đang đăng nhập từ context hoặc local storage
      details: items.map(item => ({ medicineId: parseInt(item.medicineId), quantity: parseInt(item.quantity), importPrice: parseFloat(item.importPrice) }))
    };
    setIsLoading(true);
    try {
      const response = isEdit
        ? null // TODO: await instance.put(`Import/UpdateImportBillAsync/${editInventory.importId}`, data)
        : await instance.post('Import/CreateImportBillAsync', data);
      if (!response) { showToast('info', 'Chức năng sửa đang được phát triển'); return; }
      showToast('success', response.message);
      setShowFormModal(false);
      clearFilters();
    } catch (error) { showToast('error', `Lỗi: ${error.response?.data?.message || error.message}`); } finally { setIsLoading(false); }
  }, [editInventory, clearFilters, showToast]);

  return (
    <div className='d-flex'>
      <main className='main-content flex-grow-1 p-4 d-flex flex-column gap-4'>
        {toast && <CustomToast type={toast.type} message={toast.message} onClose={hideToast} />}

        <InventoryList inventories={inventories} isLoading={isLoading} suppliers={suppliers} handleShowDeleteModal={handleShowDeleteModal} handleShowEditForm={handleShowEditForm} handleShowDetail={handleShowDetail} pageCount={pageCount} currentPage={currentPage} handlePageChange={handlePageChange} filters={filters} setFilters={setFilters} applyFilters={applyFilters} clearFilters={clearFilters} />

        <ConfirmDeleteModal isOpen={showDeleteModal} title="Xác nhận xóa" message={`Bạn có chắc muốn xóa phiếu nhập kho mã #${inventoryToDelete}?`} onConfirm={handleDelete} onCancel={handleCancelDelete} />

        <InventoryFormModal show={showFormModal} onHide={() => setShowFormModal(false)} isEditMode={!!editInventory} inventory={editInventory} onSubmit={handleFormSubmit} isLoading={isLoading} suppliers={suppliers} medicines={medicines} />

        <InventoryDetailModal show={showDetailModal} onHide={() => setShowDetailModal(false)} inventory={selectedInventory} isLoading={isLoading} />
      </main>
    </div>
  );
};

export default AdminInventory;