import React from 'react';
import { useEffect, useState, useCallback, useMemo } from 'react';
import '../../App.css';
import AdminSidebar from '../../Components/Sidebar/AdminSidebar';
import CustomToast from '../../Components/CustomToast/CustomToast';
import Loading from '../../Components/Loading/Loading';
import instance from '../../axios'; // Giả định axios instance đã được cấu hình base URL là /api
import dayjs from 'dayjs';
import { BiUserPlus, BiShow, BiPencil, BiTrash, BiLockOpen, BiLock } from 'react-icons/bi';
import { useDebounce } from 'use-debounce';
import Pagination from '../../Components/Pagination/Pagination';

// Cấu trúc state cho form, tương tự CreateUserRequest của Blazor
const initialFormState = {
  username: '',
  fullName: '',
  password: '',
  gender: '',
  email: '',
  phone: '',
  dateOfBirth: '',
  address: '',
  roleId: '',
  staffType: '',
  specialty: '',
  licenseNumber: '',
  bio: '',
};


const AdminUserManagement = () => {
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([
    { roleId: 1, roleName: 'Admin' },
    { roleId: 2, roleName: 'Receptionist' },
    { roleId: 3, roleName: 'Doctor' },
    { roleId: 4, roleName: 'Nurse' },
    { roleId: 5, roleName: 'Technician' },
    { roleId: 6, roleName: 'Patient' }
  ]);
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [pagination, setPagination] = useState({ currentPage: 1, totalPages: 1, pageSize: 10 });
  const [filters, setFilters] = useState({ search: '', gender: '', role: '', status: '' });
  const [debouncedSearchTerm] = useDebounce(filters.search, 500);

  const [modal, setModal] = useState({ type: null, user: null });
  const [formData, setFormData] = useState(initialFormState);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState(null);

  // Tạo map để chuyển đổi giữa tên vai trò và ID cho tiện lợi
  const roleNameToIdMap = useMemo(() => {
    return roles.reduce((acc, role) => {
      acc[role.roleName.toLowerCase()] = role.roleId;
      return acc;
    }, {});
  }, [roles]);

  const validateForm = () => {
    const newErrors = {};
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const phoneRegex = /^(0|\+84)(\d{9,10})$/;
    const usernameRegex = /^[a-zA-Z0-9_]{3,20}$/;
    const licenseRegex = /^[A-Z0-9]{6,15}$/;

    // Required fields validation
    if (!formData.username?.trim()) newErrors.username = 'Tên đăng nhập là bắt buộc';
    else if (!usernameRegex.test(formData.username)) newErrors.username = 'Tên đăng nhập 3-20 ký tự, chỉ chữ, số và gạch dưới';

    if (!formData.fullName?.trim()) newErrors.fullName = 'Họ tên là bắt buộc';
    else if (formData.fullName.length > 100) newErrors.fullName = 'Họ tên không quá 100 ký tự';

    if (!formData.email?.trim()) newErrors.email = 'Email là bắt buộc';
    else if (!emailRegex.test(formData.email)) newErrors.email = 'Email không hợp lệ';
    else if (formData.email.length > 100) newErrors.email = 'Email quá dài';

    if (!formData.phone?.trim()) newErrors.phone = 'Số điện thoại là bắt buộc';
    else if (!phoneRegex.test(formData.phone)) newErrors.phone = 'Số điện thoại không hợp lệ (VD: 0912345678 hoặc +84912345678)';

    if (!formData.gender) newErrors.gender = 'Giới tính là bắt buộc';

    if (!formData.roleId) newErrors.roleId = 'Vai trò là bắt buộc';

    // Date validation
    if (formData.dateOfBirth) {
      const dob = dayjs(formData.dateOfBirth);
      const minDate = dayjs().subtract(100, 'years');
      const maxDate = dayjs().subtract(18, 'years'); // Ít nhất 18 tuổi cho nhân viên

      if (dob.isAfter(dayjs())) newErrors.dateOfBirth = 'Ngày sinh không thể ở tương lai';
      else if (dob.isBefore(minDate)) newErrors.dateOfBirth = 'Ngày sinh không hợp lệ';
      else if (modal.type === 'add' && dob.isAfter(maxDate)) {
        newErrors.dateOfBirth = 'Nhân viên phải từ 18 tuổi trở lên';
      }
    }

    // Doctor-specific validations
    const selectedRole = roles.find(r => r.roleId === parseInt(formData.roleId));
    if (selectedRole?.roleName === 'Doctor') {
      if (!formData.specialty?.trim()) newErrors.specialty = 'Chuyên khoa là bắt buộc cho bác sĩ';
      else if (formData.specialty.length > 100) newErrors.specialty = 'Chuyên khoa quá dài';

      if (!formData.licenseNumber?.trim()) newErrors.licenseNumber = 'Số giấy phép là bắt buộc cho bác sĩ';
      else if (!licenseRegex.test(formData.licenseNumber)) newErrors.licenseNumber = 'Số giấy phép không hợp lệ (6-15 ký tự, chữ in hoa và số)';
    }

    // Address validation
    if (formData.address && formData.address.length > 200) {
      newErrors.address = 'Địa chỉ không quá 200 ký tự';
    }

    // Bio validation
    if (formData.bio && formData.bio.length > 500) {
      newErrors.bio = 'Tiểu sử không quá 500 ký tự';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const apiFilters = useMemo(() => ({
    search: debouncedSearchTerm,
    role: filters.role,
  }), [debouncedSearchTerm, filters.role]);

  // --- LOGIC ĐÃ CẬP NHẬT THEO BLazor ---

  const fetchUsers = useCallback(async (page = 1) => {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        role: apiFilters.role,
        search: apiFilters.search,
        page: page,
        pageSize: pagination.pageSize,
      });
      // Endpoint mới theo API
      const response = await instance.get(`Admin/GetAllUsersAsync?${params.toString()}`);


      console.log('Fetched Users Response:', response);
      // Cấu trúc response mới
      const responseData = response.content;
      const formattedUsers = (responseData.items || []).map(user => ({
        ...user,
        BirthDate: user.DateOfBirth ? dayjs(user.DateOfBirth).format('DD/MM/YYYY') : 'Chưa có',
        // Roles là một mảng string
        Role: user.Roles && user.Roles.length > 0 ? user.Roles.join(', ') : 'Chưa có',
      }));

      setUsers(formattedUsers);
      setPagination(prev => ({
        ...prev,
        currentPage: responseData.page,
        totalPages: Math.ceil(responseData.totalItems / responseData.pageSize),
      }));

    } catch (err) {
      setToast({ type: 'error', message: 'Lỗi khi tải danh sách người dùng.' });
    } finally {
      setLoading(false);
    }
  }, [apiFilters, pagination.pageSize]);

  useEffect(() => {
    fetchUsers(1);
  }, [apiFilters, fetchUsers]);



  const handleFilterChange = (e) => {
    const { name, value } = e.target;
    setFilters(prev => ({ ...prev, [name]: value }));
  };

  const handleCloseModal = () => {
    setModal({ type: null, user: null });
    setFormData(initialFormState);   // 🔥 reset dữ liệu form
    setErrors({});                   // 🔥 reset lỗi validate
  };

  const handleOpenModal = (type, user = null) => {
    setModal({ type, user });
    if (type === 'add') {
      setFormData(initialFormState);
    } else if (type === 'edit' && user) {
      // Map role name từ user object về RoleId để set cho form
      const userRoleName = user.roles && user.roles.length > 0 ? user.roles[0].toLowerCase() : '';
      const roleId = roleNameToIdMap[userRoleName] || '';

      setFormData({
        username: user.username,
        fullName: user.fullName,
        phone: user.phone,
        email: user.email,
        gender: user.gender,
        dateOfBirth: user.dateOfBirth ? dayjs(user.dateOfBirth).format('YYYY-MM-DD') : '',
        address: user.address,
        roleId: roleNameToIdMap[user.roles?.[0]?.toLowerCase()] || '',
        specialty: user.specialty,
        licenseNumber: user.licenseNumber,
        bio: user.bio,
      });

    }
  };

  const handleFormChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => {
      const newState = { ...prev, [name]: value };

      if (name === "RoleId") {
        const selectedRole = roles.find(r => r.RoleId === parseInt(value));
        // Cập nhật StaffType
        newState.StaffType = selectedRole?.RoleName === 'Bác sĩ' ? 'Bác sĩ' :
          selectedRole?.RoleName === 'Y tá' ? 'Y tá' :
            selectedRole?.RoleName === 'Lễ tân' ? 'Lễ tân' :
              selectedRole?.RoleName === 'Kĩ thuật' ? 'Kĩ thuật' : null;

        // Reset các trường của bác sĩ nếu vai trò không phải là Bác sĩ
        if (selectedRole?.RoleName !== 'Bác sĩ') {
          newState.Specialty = '';
          newState.LicenseNumber = '';
          newState.Bio = '';
        }
      }
      return newState;
    });
  };


  const handleFormSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      setToast({ type: 'error', message: 'Vui lòng kiểm tra lại thông tin!' });
      return; // ❌ dừng lại, không call API
    }
    setLoading(true);

    const { type, user } = modal;
    const isEditing = type === 'edit';

    // Endpoint và method được cập nhật theo API
    const url = isEditing ? `/Admin/UpdateUser/${user.userId}` : '/Admin/CreateUser';
    const method = isEditing ? 'put' : 'post';

    // Mật khẩu mặc định khi tạo mới là SĐT, backend sẽ hash
    const dataToSend = { ...formData };
    if (!isEditing) {
      dataToSend.Password = formData.Phone;
    } else {
      delete dataToSend.Password; // Không gửi mật khẩu khi cập nhật
    }

    try {
      const responseData = await instance[method](url, dataToSend);
      if (responseData.status === 'Success') {
        setToast({ type: 'success', message: responseData.message || 'Thao tác thành công!' });
        handleCloseModal();
        fetchUsers(pagination.currentPage);
      } else {
        setToast({ type: 'error', message: responseData.message || 'Có lỗi xảy ra.' });
      }
    } catch (err) {
      const errorMessage = err.response?.message || err.response?.title || 'Có lỗi xảy ra.';
      setToast({ type: 'error', message: errorMessage });
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteUser = async () => {
    setLoading(true);
    try {
      // Endpoint mới
      await instance.delete(`/Admin/User/${modal.user.userId}`);
      setToast({ type: 'success', message: 'Xóa người dùng thành công!' });
      handleCloseModal();
      const newPage = users.length === 1 && pagination.currentPage > 1 ? pagination.currentPage - 1 : pagination.currentPage;
      fetchUsers(newPage);
    } catch (err) {
      setToast({ type: 'error', message: err.response?.message || 'Lỗi khi xóa người dùng.' });
    } finally {
      setLoading(false);
    }
  };

  const handleToggleStatus = async () => {
    setLoading(true);
    const { user } = modal;
    try {
      // Endpoint mới và cần gửi body
      const response = await instance.put(`/Admin/toggle-active/${user.userId}`, {
        Active: !user.isActive
      });
      setToast({ type: 'success', message: response.message || 'Thay đổi trạng thái thành công!' });
      handleCloseModal();
      fetchUsers(pagination.currentPage);
    } catch (err) {
      setToast({ type: 'error', message: err.response?.message || 'Lỗi khi thay đổi trạng thái.' });
    } finally {
      setLoading(false);
    }
  };

  // --- PHẦN RENDER GIAO DIỆN (UI) GIỮ NGUYÊN NHƯNG CẬP NHẬT FORM VÀ DETAIL ---

  const renderModal = () => {
    if (!modal.type) return null;

    const modalLayout = (title, body, footer, maxWidth = '700px') => (
      <>
        <div className="modal-backdrop fade show"></div>
        <div className="modal fade show d-block" tabIndex="-1" onClick={handleCloseModal}>
          <div className="modal-dialog modal-dialog-centered" style={{ maxWidth }} onClick={e => e.stopPropagation()}>
            <div className="modal-content border-0 shadow-lg">
              <div className="modal-header">
                <h5 className="modal-title fw-semibold">{title}</h5>
                <button type="button" className="btn-close" onClick={handleCloseModal}></button>
              </div>
              <div className="modal-body">{body}</div>
              {footer && <div className="modal-footer">{footer}</div>}
            </div>
          </div>
        </div>
      </>
    );

    switch (modal.type) {
      case 'add':
      case 'edit':
        const isEditing = modal.type === 'edit';
        const selectedRoleName = roles.find(r => r.roleId === parseInt(formData.roleId))?.roleName;

        return modalLayout(
          isEditing ? 'Cập Nhật Thông Tin' : 'Thêm Người Dùng Mới',
          <form onSubmit={handleFormSubmit}>
            <div className="row g-3">
              <div className="col-md-6 mb-3"><label className="form-label">Tên đăng nhập</label><input type="text" name="username" value={formData.username || ''} onChange={handleFormChange} className="form-control" required disabled={isEditing} /></div>
              <div className="col-md-6 mb-3"><label className="form-label">Họ tên</label><input type="text" name="fullName" value={formData.fullName || ''} onChange={handleFormChange} className="form-control" required />{errors.fullName && <div className="text-danger small">{errors.fullName}</div>}
              </div>
              {/* Mật khẩu mặc định là SĐT, không cần input khi thêm */}
              {/* {!isEditing && <div className="col-12 mb-3"><label className="form-label">Mật khẩu</label><input type="password" name="Password" value={formData.Password || ''} onChange={handleFormChange} className="form-control" required /></div>} */}
              <div className="col-md-6 mb-3"><label className="form-label">Email</label><input type="email" name="email" value={formData.email || ''} onChange={handleFormChange} className="form-control" required /> {errors.email && <div className="text-danger small">{errors.email}</div>}
              </div>
              <div className="col-md-6 mb-3"><label className="form-label">Số điện thoại</label><input type="tel" name="phone" value={formData.phone || ''} onChange={handleFormChange} className="form-control" required />{errors.phone && <div className="text-danger small">{errors.phone}</div>}
              </div>
              <div className="col-md-6 mb-3"><label className="form-label">Ngày sinh</label><input type="date" name="dateOfBirth" value={formData.dateOfBirth || ''} onChange={handleFormChange} className="form-control" />{errors.dateOfBirth && <div className="text-danger small">{errors.dateOfBirth}</div>}
              </div>
              <div className="col-md-6 mb-3"><label className="form-label">Giới tính</label><select name="gender" value={formData.gender || ''} onChange={handleFormChange} className="form-select" required><option value="">Chọn giới tính</option><option value="Nam">Nam</option><option value="Nữ">Nữ</option></select></div>
              <div className="col-12 mb-3"><label className="form-label">Địa chỉ</label><input type="text" name="address" value={formData.address || ''} onChange={handleFormChange} className="form-control" />{errors.address && <div className="text-danger small">{errors.address}</div>}
              </div>

              <div className="col-12 mb-3"><label className="form-label">Vai trò</label>
                <select name="roleId" value={formData.roleId || ''} onChange={handleFormChange} className="form-select" required>
                  <option value="">Chọn vai trò</option>
                  {roles.map(r => <option key={r.roleId} value={r.roleId}>{r.roleName}</option>)}
                </select>
              </div>

              {/* Conditional fields for Doctor role */}
              {selectedRoleName === 'Bác sĩ' && (
                <>
                  <hr />
                  <h6 className='text-primary'>Thông tin Bác sĩ</h6>
                  <div className="col-md-6 mb-3"><label className="form-label">Chuyên khoa</label><input type="text" name="Specialty" value={formData.specialty || ''} onChange={handleFormChange} className="form-control" /></div>
                  <div className="col-md-6 mb-3"><label className="form-label">Số giấy phép</label><input type="text" name="LicenseNumber" value={formData.licenseNumber || ''} onChange={handleFormChange} className="form-control" /></div>
                  <div className="col-12 mb-3"><label className="form-label">Tiểu sử/Ghi chú</label><textarea name="Bio" value={formData.bio || ''} onChange={handleFormChange} className="form-control" rows="3"></textarea></div>
                </>
              )}
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={handleCloseModal}>Hủy</button>
              <button type="submit" className="btn btn-primary">{isEditing ? 'Lưu thay đổi' : 'Thêm mới'}</button>
            </div>
          </form>,
          null
        );

      case 'delete':
        return modalLayout(
          'Xác Nhận Xóa',
          <>
            <p>Bạn có chắc chắn muốn xóa người dùng <strong>{modal.user.FullName}</strong>?</p>
            <p className="text-muted small">Hành động này không thể hoàn tác.</p>
          </>,
          <>
            <button className="btn btn-secondary" onClick={handleCloseModal}>Hủy</button>
            <button className="btn btn-danger" onClick={handleDeleteUser}>Xác Nhận Xóa</button>
          </>,
          '450px'
        );

      case 'status':
        return modalLayout(
          'Xác Nhận',
          <p>Bạn có chắc muốn <strong>{modal.user.isActive ? 'vô hiệu hóa' : 'kích hoạt'}</strong> tài khoản của <strong>{modal.user.fullName}</strong>?</p>,
          <>
            <button className="btn btn-secondary" onClick={handleCloseModal}>Hủy</button>
            <button className={`btn ${modal.user.isActive ? 'btn-warning' : 'btn-success'}`} onClick={handleToggleStatus}>Xác Nhận</button>
          </>,
          '450px'
        );

      case 'detail':
        const InfoRow = ({ label, value }) => (
          value ? <div className="d-flex justify-content-between py-2 border-bottom">
            <span className="text-muted">{label}:</span>
            <span className="fw-semibold text-dark text-end">{value}</span>
          </div> : null
        );
        return modalLayout(
          'Chi Tiết Người Dùng',
          <>
            <InfoRow label="ID" value={modal.user.userId} />
            <InfoRow label="Tên đăng nhập" value={modal.user.username} />
            <InfoRow label="Họ tên" value={modal.user.fullName} />
            <InfoRow label="Email" value={modal.user.email} />
            <InfoRow label="SĐT" value={modal.user.phone} />
            <InfoRow label="Giới tính" value={modal.user.gender} />
            <InfoRow label="Ngày sinh" value={modal.user.birthDate} />
            <InfoRow label="Địa chỉ" value={modal.user.address || 'Chưa có'} />
            <InfoRow label="Vai trò" value={modal.user.roles} />
            <InfoRow label="Loại nhân viên" value={modal.user.staffType} />
            <InfoRow label="Chuyên khoa" value={modal.user.specialty} />
            <InfoRow label="Số giấy phép" value={modal.user.licenseNumber} />
            <InfoRow label="Tiểu sử" value={modal.user.bio} />
            <div className="d-flex justify-content-between py-2 border-bottom">
              <span className="text-muted">Trạng thái:</span>
              <span className={`badge my-auto ${modal.user.isActive ? 'bg-success-soft text-success' : 'bg-secondary-soft text-secondary'}`}>{modal.user.isActive ? 'Hoạt động' : 'Vô hiệu hóa'}</span>
            </div>
          </>,
          <button className="btn btn-outline-secondary" onClick={handleCloseModal}>Đóng</button>
        );

      default:
        return null;
    }
  };

  return (
    <div className="d-flex">
      <main className="main-content flex-grow-1 p-4 d-flex flex-column gap-4">
        {toast && <CustomToast type={toast.type} message={toast.message} onClose={() => setToast(null)} />}
        {loading && <Loading isLoading={loading} />}

        <header className="d-flex justify-content-between align-items-center flex-shrink-0">
          <h1 className="h4 mb-0">Quản Lý Người Dùng</h1>
          <button className="btn btn-primary d-flex align-items-center gap-2" onClick={() => handleOpenModal('add')}>
            <BiUserPlus size={20} /> Thêm Người Dùng
          </button>
        </header>

        <div className="card shadow-sm border-0 flex-shrink-0">
          <div className="card-body p-4">
            <div className="row g-3">
              <div className="col-md-6"><input type="text" name="search" className="form-control" placeholder="Tìm theo tên, email..." value={filters.search} onChange={handleFilterChange} /></div>
              <div className="col-md-3"><select name="role" className="form-select" value={filters.role} onChange={handleFilterChange}><option value="">Tất cả vai trò</option>{roles.map(r => <option key={r.RoleId} value={r.RoleName}>{r.RoleName}</option>)}</select></div>
              {/* Các filter khác có thể thêm ở đây nếu API hỗ trợ */}
            </div>
          </div>
        </div>

        <div className="card shadow-sm border-0 table-panel">
          <>
            <div className="table-responsive-container">
              <table className="table table-hover clinic-table mb-0">
                <thead>
                  <tr>
                    <th className="px-4">ID</th><th>Họ tên</th><th>Email</th><th>SĐT</th>
                    <th>Vai trò</th>
                    <th className="text-center">Trạng thái</th>
                    <th className="text-center px-4">Hành động</th>
                  </tr>
                </thead>
                <tbody>
                  {users.length > 0 ? users.map(user => (
                    <tr key={user.userId}>
                      <td className="px-4"><span className='user-id'>{`#${user.userId}`}</span></td>
                      <td className="fw-semibold">{user.fullName || 'Chưa cập nhật'}</td>
                      <td>{user.email}</td>
                      <td>{user.phone}</td>
                      <td>{user.roles}</td>
                      <td className="text-center">
                        <span className={`badge rounded-pill fs-6 fw-semibold ${user.isActive ? 'bg-success-soft' : 'bg-secondary-soft'}`}>
                          {user.isActive ? 'Hoạt động' : 'Vô hiệu hóa'}
                        </span>
                      </td>
                      <td className="text-center px-4">
                        <div className="d-flex gap-2 justify-content-center">
                          <button className="btn btn-lg btn-light" title="Chi tiết" onClick={() => handleOpenModal('detail', user)}><BiShow /></button>
                          <button className="btn btn-lg btn-light" title="Sửa" onClick={() => handleOpenModal('edit', user)}><BiPencil /></button>
                          <button className={`btn btn-lg btn-light text-${user.isActive ? 'warning' : 'success'}`} title={user.isActive ? 'Vô hiệu hóa' : 'Kích hoạt'} disabled={user.roles[0] === "Admin"} onClick={() => handleOpenModal('status', user)}>
                            {user.isActive ? <BiLock /> : <BiLockOpen />}
                          </button>
                          <button className="btn btn-lg btn-light text-danger" title="Xóa" onClick={() => handleOpenModal('delete', user)} disabled={user.roles[0] === "Admin"}><BiTrash /></button>
                        </div>
                      </td>
                    </tr>
                  )) : (
                    <tr><td colSpan="7" className="text-center p-5 text-muted">Không tìm thấy người dùng.</td></tr>
                  )}
                </tbody>
              </table>
            </div>

            {pagination.totalPages > 1 && (
              <div className="card-footer p-3 border-0 flex-shrink-0">
                <Pagination
                  pageCount={pagination.totalPages}
                  currentPage={pagination.currentPage - 1} // chuyển về 0-based cho UI
                  onPageChange={({ selected }) => fetchUsers(selected + 1)} // chuyển lại 1-based cho API
                  isLoading={loading}
                />
              </div>
            )}
          </>
        </div>

        {renderModal()}
      </main>
    </div>
  );
};

export default AdminUserManagement;