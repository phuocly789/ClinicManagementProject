import React, { useState, useEffect, useCallback, memo, useRef } from 'react';
import Pagination from '../../Components/Pagination/Pagination';
import ConfirmDeleteModal from '../../Components/CustomToast/DeleteConfirmModal';
import CustomToast from '../../Components/CustomToast/CustomToast';
import { Pencil, Trash, Search, PlusCircle, X, Mail, Phone, MapPin } from 'lucide-react';
import Loading from '../../Components/Loading/Loading';
import instance from '../../axios';
import '../../App.css';

// --- COMPONENT: Giao diện Danh sách Nhà cung cấp ---
const SupplierList = memo(({ suppliers, isLoading, handleShowDeleteModal, handleShowEditForm, pageCount, currentPage, handlePageChange, filters, setFilters, applyFilters, clearFilters }) => {
const handleKeyDown = (e) => { if (e.key === 'Enter') { e.preventDefault(); applyFilters(); } };

    return (
        <>
            <header className="d-flex justify-content-between align-items-center flex-shrink-0">
                <h1 className="h4 mb-0">Quản Lý Nhà Cung Cấp</h1>
                <button className="btn btn-primary d-flex align-items-center gap-2" onClick={() => handleShowEditForm(null)}><PlusCircle size={16} /> Thêm Nhà Cung Cấp</button>
            </header>

            <div className="card shadow-sm border-0 flex-shrink-0">
                <div className="card-body p-4">
                    <form onSubmit={(e) => { e.preventDefault(); applyFilters(); }}>
                        <div className="row g-3 align-items-end">
                            <div className="col-md-8">
                                <label className="form-label small text-muted">Tìm theo tên nhà cung cấp</label>
                                <div className="input-group">
                                    <span className="input-group-text"><Search /></span>
                                    <input type="text" className="form-control" placeholder="Nhập tên và nhấn Enter..." value={filters.search} onChange={(e) => setFilters(p => ({ ...p, search: e.target.value }))} onKeyDown={handleKeyDown} />
                                </div>
                            </div>
                            <div className="col-md-4 d-flex justify-content-end gap-2">
                                <button type="submit" className="btn btn-primary d-flex align-items-center gap-2"><Search /> Lọc</button>
                                <button type="button" className="btn btn-outline-danger d-flex align-items-center gap-2" onClick={clearFilters}><X /> Xóa Lọc</button>
                            </div>
                        </div>
                    </form>
                </div>
            </div>

            <div className="card shadow-sm border-0 table-panel">
                {isLoading && suppliers.length === 0 ? (<Loading isLoading={isLoading} />) : (
                    <>
                        <div className="table-responsive-container">
                            <table className="table table-hover clinic-table mb-0 text-center">
                                <thead>
                                    <tr>
                                        <th className="px-4">Mã NCC</th>
                                        <th>Tên Nhà Cung Cấp</th>
                                        <th>Email</th>
                                        <th>Số Điện Thoại</th>
                                        <th>Địa Chỉ</th>
                                        <th className="text-center px-4">Hành Động</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {!isLoading && suppliers.length === 0 ? (<tr><td colSpan="6" className="text-center p-5 text-muted">Không có dữ liệu</td></tr>) : (
                                        suppliers.map((supplier) => (
                                            <tr key={supplier.supplierId}>
                                                <td className="px-4"><span className="user-id">#{supplier.supplierId}</span></td>
                                                <td className="fw-bold">{supplier.supplierName}</td>
                                                <td>{supplier.contactEmail}</td>
                                                <td>{supplier.contactPhone}</td>
                                                <td title={supplier.address}>{supplier.address?.length > 100 ? supplier.address.substring(0, 100) + '...' : supplier.address}</td>
                                                <td className="text-center px-4">
                                                    <button className="btn btn-light btn-sm me-2" title="Sửa" onClick={() => handleShowEditForm(supplier)}><Pencil size={16} /></button>
                                                    <button className="btn btn-light btn-sm text-danger" title="Xóa" onClick={() => handleShowDeleteModal(supplier.supplierId)}><Trash size={16} /></button>
                                                </td>
                                            </tr>
                                        ))
                                    )}
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

// --- COMPONENT: Modal Form Thêm/Sửa Nhà cung cấp ---
const SupplierFormModal = memo(({ show, onHide, isEditMode, supplier, onSubmit, isLoading }) => {
    const formRef = useRef(null);
    if (!show) return null;

    const handleSubmit = (e) => {
        e.preventDefault();
        onSubmit(e);
    };

    return (
        <><div className="modal-backdrop fade show"></div>
            <div className="modal fade show d-block" tabIndex="-1" onClick={onHide}>
                <div className="modal-dialog modal-dialog-centered modal-lg" onClick={(e) => e.stopPropagation()}>
                    <form ref={formRef} onSubmit={handleSubmit} className="modal-content">
                        <div className="modal-header">
                            <h5 className="modal-title">{isEditMode ? `Sửa Nhà Cung Cấp #${supplier.supplierId}` : 'Tạo Nhà Cung Cấp Mới'}</h5>
                            <button type="button" className="btn-close" onClick={onHide}></button>
                        </div>
                        <div className="modal-body">
                            <div className="row g-3">
                                <div className="col-12"><label className="form-label">Tên Nhà Cung Cấp</label><div className="input-group"><span className="input-group-text"><Search /></span><input type="text" name="supplierName" defaultValue={supplier?.supplierName || ''} className="form-control" required /></div></div>
                                <div className="col-md-6"><label className="form-label">Email Liên Hệ</label><div className="input-group"><span className="input-group-text"><Mail /></span><input type="email" name="contactEmail" defaultValue={supplier?.contactEmail || ''} className="form-control" required /></div></div>
                                <div className="col-md-6"><label className="form-label">Số Điện Thoại</label><div className="input-group"><span className="input-group-text"><Phone /></span><input type="tel" name="contactPhone" defaultValue={supplier?.contactPhone || ''} className="form-control" required /></div></div>
                                <div className="col-12"><label className="form-label">Địa Chỉ</label><div className="input-group"><span className="input-group-text"><MapPin /></span><input type="text" name="address" defaultValue={supplier?.address || ''} className="form-control" required /></div></div>
                                <div className="col-12"><label className="form-label">Mô Tả</label><textarea name="description" defaultValue={supplier?.description || ''} className="form-control" rows="3"></textarea></div>
                            </div>
                        </div>
                        <div className="modal-footer">
                            <button type="button" className="btn btn-secondary" onClick={onHide}>Hủy</button>
                            <button type="submit" className="btn btn-primary" disabled={isLoading}>{isLoading ? 'Đang lưu...' : 'Lưu Thay Đổi'}</button>
                        </div>
                    </form>
                </div>
            </div></>
    );
});

// --- COMPONENT CHÍNH: AdminSupplier ---
const initialFilters = { search: '' };

const AdminSupplier = () => {
    const [suppliers, setSuppliers] = useState([]);
    const [currentPage, setCurrentPage] = useState(0);
    const [pageCount, setPageCount] = useState(0);
    const [isLoading, setIsLoading] = useState(false);
    const [toast, setToast] = useState(null);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [supplierToDelete, setSupplierToDelete] = useState(null);
    const [showFormModal, setShowFormModal] = useState(false);
    const [editSupplier, setEditSupplier] = useState(null);
    const [filters, setFilters] = useState(initialFilters);

    const showToast = useCallback((type, message) => { setToast({ type, message }); }, []);
    const hideToast = useCallback(() => { setToast(null); }, []);

    const fetchSuppliers = useCallback(async (page = 1, currentFilters = filters) => {
        setIsLoading(true);
        try {
            const params = {
                page,
                pageSize: 10,
                search: currentFilters.search || null,
            };
            const response = await instance.get('Supplier/GetAllSupplierAsync', { params });
            if (response?.content) {
                setSuppliers(response.content.items || []);
                setPageCount(Math.ceil(response.content.totalItems / response.content.pageSize));
                setCurrentPage(response.content.page - 1);
            }
        } catch (error) {
            showToast('error', `Lỗi tải nhà cung cấp: ${error.response?.data?.message || error.message}`);
        } finally {
            setIsLoading(false);
        }
    }, [showToast, filters]);

    useEffect(() => {
        fetchSuppliers(1);
    }, []);

    const applyFilters = () => { setCurrentPage(0); fetchSuppliers(1, filters); };
    const clearFilters = useCallback(() => { setFilters(initialFilters); setCurrentPage(0); fetchSuppliers(1, initialFilters); }, [fetchSuppliers]);
    const handlePageChange = useCallback(({ selected }) => { fetchSuppliers(selected + 1, filters); }, [fetchSuppliers, filters]);
    const handleShowDeleteModal = (id) => { setSupplierToDelete(id); setShowDeleteModal(true); };
    const handleCancelDelete = () => { setSupplierToDelete(null); setShowDeleteModal(false); };
    const handleShowEditForm = (supplier) => { setEditSupplier(supplier); setShowFormModal(true); };

    const handleDelete = useCallback(async () => {
        if (!supplierToDelete) return;
        setIsLoading(true);
        try {
            const res = await instance.delete(`Supplier/DeleteSupplierAsync/${supplierToDelete}`);
            showToast('success', res.message || `Đã xóa nhà cung cấp #${supplierToDelete}`);
            setShowDeleteModal(false);
            setSupplierToDelete(null);
            clearFilters();
        } catch (error) {
            showToast('error', `Lỗi khi xóa: ${error.response?.data?.message || error.message}`);
        } finally {
            setIsLoading(false);
        }
    }, [supplierToDelete, clearFilters, showToast]);

    const handleFormSubmit = useCallback(async (e) => {
        e.preventDefault();
        const isEdit = !!editSupplier;
        const formData = new FormData(e.target);
        const data = {
            supplierName: formData.get('supplierName'),
            contactEmail: formData.get('contactEmail'),
            contactPhone: formData.get('contactPhone'),
            address: formData.get('address'),
            description: formData.get('description'),
        };

        setIsLoading(true);
        try {
            const response = isEdit
                ? await instance.put(`Supplier/UpdateSupplierAsync/${editSupplier.supplierId}`, data)
                : await instance.post('Supplier/CreateSupplierAsync', data);

            showToast('success', response.message);
            setShowFormModal(false);
            setEditSupplier(null);
            clearFilters();
        } catch (error) {
            showToast('error', `Lỗi: ${error.response?.data?.message || error.message}`);
        } finally {
            setIsLoading(false);
        }
    }, [editSupplier, clearFilters, showToast]);

    return (
    
            <main className='main-content flex-grow-1 p-4 d-flex flex-column gap-4'>
                {toast && <CustomToast type={toast.type} message={toast.message} onClose={hideToast} />}

                <SupplierList
                    suppliers={suppliers}
                    isLoading={isLoading}
                    handleShowDeleteModal={handleShowDeleteModal}
                    handleShowEditForm={handleShowEditForm}
                    pageCount={pageCount}
                    currentPage={currentPage}
                    handlePageChange={handlePageChange}
                    filters={filters}
                    setFilters={setFilters}
                    applyFilters={applyFilters}
                    clearFilters={clearFilters}
                />

                <ConfirmDeleteModal
                    isOpen={showDeleteModal}
                    title="Xác nhận xóa"
                    message={`Bạn có chắc muốn xóa nhà cung cấp mã #${supplierToDelete}? Thao tác này có thể ảnh hưởng đến các phiếu nhập kho liên quan.`}
                    onConfirm={handleDelete}
                    onCancel={handleCancelDelete}
                />

                <SupplierFormModal
                    show={showFormModal}
                    onHide={() => setShowFormModal(false)}
                    isEditMode={!!editSupplier}
                    supplier={editSupplier}
                    onSubmit={handleFormSubmit}
                    isLoading={isLoading}
                />
            </main>
 
    );
};

export default AdminSupplier;