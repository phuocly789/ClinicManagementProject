import React, { useState, useEffect, useCallback, memo, useRef } from 'react';
import Pagination from '../../Components/Pagination/Pagination';
import ConfirmDeleteModal from '../../Components/CustomToast/DeleteConfirmModal';
import CustomToast from '../../Components/CustomToast/CustomToast';
import { Pencil, Trash, Search, DollarSign, PlusCircle, X } from 'lucide-react';
import Loading from '../../Components/Loading/Loading';
import instance from '../../axios';
import '../../App.css';

// --- Hằng số và Helper Functions ---
const formatVND = (value) => {
    if (value === null || value === undefined) return 'N/A';
    return Number(value).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
};

// --- Các loại dịch vụ ---
// Mảng tĩnh cho các loại dịch vụ, bạn có thể thay đổi nếu cần
const serviceTypes = ['Khám bệnh', 'Examination', 'Test', 'Procedure', 'Khác'];

// --- COMPONENT: Giao diện Danh sách Dịch vụ ---
const ServiceList = memo(({ services, isLoading, handleShowDeleteModal, handleShowEditForm, pageCount, currentPage, handlePageChange, filters, setFilters, applyFilters, clearFilters }) => {
    const handleKeyDown = (e) => { if (e.key === 'Enter') { e.preventDefault(); applyFilters(); } };

    return (
        <>
            <header className="d-flex justify-content-between align-items-center flex-shrink-0">
                <h1 className="h4 mb-0">Quản Lý Dịch Vụ</h1>
                <button className="btn btn-primary d-flex align-items-center gap-2" onClick={() => handleShowEditForm(null)}><PlusCircle size={16} /> Thêm Dịch Vụ</button>
            </header>
            <div className="card shadow-sm border-0 flex-shrink-0">
                <div className="card-body p-4">
                    <form onSubmit={(e) => { e.preventDefault(); applyFilters(); }}>
                        <div className="row g-3">
                            <div className="col-md-4"><label className="form-label small text-muted">Tìm theo tên dịch vụ</label><div className="input-group"><span className="input-group-text"><Search /></span><input type="text" className="form-control" placeholder="Nhập tên và nhấn Enter..." value={filters.search} onChange={(e) => setFilters(p => ({ ...p, search: e.target.value }))} onKeyDown={handleKeyDown} /></div></div>
                            <div className="col-md-4"><label className="form-label small text-muted">Loại dịch vụ</label><select className="form-select" value={filters.serviceType} onChange={(e) => setFilters(p => ({ ...p, serviceType: e.target.value }))} onKeyDown={handleKeyDown}><option value="">Tất cả</option>{serviceTypes.map(type => <option key={type} value={type}>{type}</option>)}</select></div>
                            <div className="col-md-4"><label className="form-label small text-muted">Giá tiền</label><div className="input-group"><span className="input-group-text"><DollarSign size={16} /></span><input type="number" className="form-control" placeholder="Từ" value={filters.minPrice} onChange={(e) => setFilters(p => ({ ...p, minPrice: e.target.value }))} onKeyDown={handleKeyDown} /><span className="input-group-text">→</span><input type="number" className="form-control" placeholder="Đến" value={filters.maxPrice} onChange={(e) => setFilters(p => ({ ...p, maxPrice: e.target.value }))} onKeyDown={handleKeyDown} /></div></div>
                            <div className="col-md-12 d-flex align-items-end justify-content-end gap-2 mt-3">
                                <button type="submit" className="btn btn-primary d-flex align-items-center gap-2"><Search /> Lọc</button>
                                <button type="button" className="btn btn-outline-danger d-flex align-items-center gap-2" onClick={clearFilters}><X /> Xóa Lọc</button>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
            <div className="card shadow-sm border-0 table-panel">
                {isLoading && services.length === 0 ? (<Loading isLoading={isLoading} />) : (
                    <>
                        <div className="table-responsive-container">
                            <table className="table table-hover clinic-table mb-0 text-center">
                                <thead><tr><th className="px-4">Mã DV</th><th>Tên Dịch Vụ</th><th>Loại Dịch Vụ</th><th className="text-end">Giá Tiền</th><th>Mô Tả</th><th className="text-center px-4">Hành Động</th></tr></thead>
                                <tbody>
                                    {!isLoading && services.length === 0 ? (<tr><td colSpan="6" className="text-center p-5 text-muted">Không có dữ liệu</td></tr>) : (services.map((service) => (<tr key={service.serviceId}><td className="px-4"><span className="user-id">#{service.serviceId}</span></td><td className='fw-bold'>{service.serviceName}</td><td>{service.serviceType}</td><td className="text-end fw-semibold">{formatVND(service.price)}</td><td title={service.description}>{service.description?.length > 100 ? service.description.substring(0, 100) + '...' : service.description || '—'}</td><td className="text-center px-4"><button className="btn btn-light btn-sm me-2" title="Sửa" onClick={() => handleShowEditForm(service)}><Pencil size={16} /></button><button className="btn btn-light btn-sm text-danger" title="Xóa" onClick={() => handleShowDeleteModal(service.serviceId)}><Trash size={16} /></button></td></tr>)))}
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

// --- COMPONENT: Modal Form Thêm/Sửa Dịch vụ ---
const ServiceFormModal = memo(({ show, onHide, isEditMode, service, onSubmit, isLoading }) => {
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
                        <div className="modal-header"><h5 className="modal-title">{isEditMode ? `Sửa Dịch Vụ #${service.serviceId}` : 'Tạo Dịch Vụ Mới'}</h5><button type="button" className="btn-close" onClick={onHide}></button></div>
                        <div className="modal-body">
                            <div className="row g-3">
                                <div className="col-md-12"><label className="form-label">Tên Dịch Vụ</label><input type="text" name="serviceName" defaultValue={service?.serviceName || ''} className="form-control" placeholder="VD: Khám tổng quát" required /></div>
                                <div className="col-md-6"><label className="form-label">Loại Dịch Vụ</label><select name="serviceType" defaultValue={service?.serviceType || ''} className="form-select" required><option value="" disabled>Chọn loại dịch vụ</option>{serviceTypes.map(type => <option key={type} value={type}>{type}</option>)}</select></div>
                                <div className="col-md-6"><label className="form-label">Giá Tiền (VNĐ)</label><input type="number" name="price" defaultValue={service?.price || ''} className="form-control" placeholder="VD: 150000" min="0" required /></div>
                                <div className="col-12"><label className="form-label">Mô Tả</label><textarea name="description" defaultValue={service?.description || ''} className="form-control" rows="3" placeholder="Mô tả chi tiết về dịch vụ..."></textarea></div>
                            </div>
                        </div>
                        <div className="modal-footer"><button type="button" className="btn btn-secondary" onClick={onHide}>Hủy</button><button type="submit" className="btn btn-primary" disabled={isLoading}>{isLoading ? 'Đang lưu...' : 'Lưu Thay Đổi'}</button></div>
                    </form>
                </div>
            </div></>
    );
});

// --- COMPONENT CHÍNH: AdminService ---
const initialFilters = { search: '', serviceType: '', minPrice: '', maxPrice: '' };

const AdminService = () => {
    const [services, setServices] = useState([]);
    const [currentPage, setCurrentPage] = useState(0);
    const [pageCount, setPageCount] = useState(0);
    const [isLoading, setIsLoading] = useState(false);
    const [toast, setToast] = useState(null);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [serviceToDelete, setServiceToDelete] = useState(null);
    const [showFormModal, setShowFormModal] = useState(false);
    const [editService, setEditService] = useState(null);
    const [filters, setFilters] = useState(initialFilters);

    const showToast = useCallback((type, message) => { setToast({ type, message }); }, []);
    const hideToast = useCallback(() => { setToast(null); }, []);

    const fetchServices = useCallback(async (page = 1, currentFilters = filters) => {
        setIsLoading(true);
        try {
            const params = {
                page, pageSize: 10,
                search: currentFilters.search || null,
                serviceType: currentFilters.serviceType || null,
                minPrice: currentFilters.minPrice || null,
                maxPrice: currentFilters.maxPrice || null,
            };
            const response = await instance.get('Service/GetAllServicesAsync', { params });
            if (response?.content) {
                setServices(response.content.items || []);
                setPageCount(Math.ceil(response.content.totalItems / response.content.pageSize));
                setCurrentPage(response.content.page - 1);
            }
        } catch (error) { showToast('error', `Lỗi tải dịch vụ: ${error.response?.data?.message || error.message}`); } finally { setIsLoading(false); }
    }, [showToast, filters]);

    useEffect(() => { fetchServices(1); }, []);

    const applyFilters = () => { setCurrentPage(0); fetchServices(1, filters); };
    const clearFilters = useCallback(() => { setFilters(initialFilters); setCurrentPage(0); fetchServices(1, initialFilters); }, [fetchServices]);
    const handlePageChange = useCallback(({ selected }) => { fetchServices(selected + 1, filters); }, [fetchServices, filters]);
    const handleShowDeleteModal = (id) => { setServiceToDelete(id); setShowDeleteModal(true); };
    const handleCancelDelete = () => { setServiceToDelete(null); setShowDeleteModal(false); };
    const handleShowEditForm = (service) => { setEditService(service); setShowFormModal(true); };

    const handleDelete = useCallback(async () => {
        if (!serviceToDelete) return;
        setIsLoading(true);
        try {
            const res = await instance.delete(`Service/DeleteServiceAsync/${serviceToDelete}`);
            showToast('success', res.message || `Đã xóa dịch vụ #${serviceToDelete}`);
            setShowDeleteModal(false);
            setServiceToDelete(null);
            clearFilters();
        } catch (error) { showToast('error', `Lỗi khi xóa: ${error.response?.data?.message || error.message}`); } finally { setIsLoading(false); }
    }, [serviceToDelete, clearFilters, showToast]);

    const handleFormSubmit = useCallback(async (e) => {
        e.preventDefault();
        const isEdit = !!editService;
        const formData = new FormData(e.target);
        const data = {
            serviceName: formData.get('serviceName'),
            serviceType: formData.get('serviceType'),
            price: parseFloat(formData.get('price')),
            description: formData.get('description'),
        };

        setIsLoading(true);
        try {
            const response = isEdit
                ? await instance.put(`Service/UpdateServiceAsync/${editService.serviceId}`, data)
                : await instance.post('Service/CreateServiceAsync', data);
            
            showToast('success', response.message);
            setShowFormModal(false);
            setEditService(null);
            clearFilters();
        } catch (error) { showToast('error', `Lỗi: ${error.response?.data?.message || error.message}`); } finally { setIsLoading(false); }
    }, [editService, clearFilters, showToast]);

    return (
        <div className='d-flex'>
            <main className='main-content flex-grow-1 p-4 d-flex flex-column gap-4'>
                {toast && <CustomToast type={toast.type} message={toast.message} onClose={hideToast} />}
                
                <ServiceList
                    services={services} isLoading={isLoading}
                    handleShowDeleteModal={handleShowDeleteModal}
                    handleShowEditForm={handleShowEditForm}
                    pageCount={pageCount} currentPage={currentPage}
                    handlePageChange={handlePageChange}
                    filters={filters} setFilters={setFilters}
                    applyFilters={applyFilters} clearFilters={clearFilters}
                />
                
                <ConfirmDeleteModal
                    isOpen={showDeleteModal}
                    title="Xác nhận xóa"
                    message={`Bạn có chắc muốn xóa dịch vụ mã #${serviceToDelete}? Thao tác này không thể hoàn tác.`}
                    onConfirm={handleDelete}
                    onCancel={handleCancelDelete}
                />
                
                <ServiceFormModal
                    show={showFormModal}
                    onHide={() => setShowFormModal(false)}
                    isEditMode={!!editService}
                    service={editService}
                    onSubmit={handleFormSubmit}
                    isLoading={isLoading}
                />
            </main>
        </div>
    );
};

export default AdminService;