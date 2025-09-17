using ClinicManagement_API.Models;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public interface IAdminService
{
    Task<PagedResult<UserDTO>> GetAllUsersAsync(string role, string search, int page, int pageSize);
    Task ToggleUserActiveAsync(int userId, bool active);
    Task DeleteUserAsync(int userId);
}

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IMedicalStaffRepository _medicalStaffRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUnitOfWork uow,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IMedicalStaffRepository medicalStaffRepository,
        IPatientRepository patientRepository,
        ILogger<AdminService> logger
    )
    {
        _uow = uow;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _medicalStaffRepository = medicalStaffRepository;
        _patientRepository = patientRepository;
        _logger = logger;
    }

    public async Task<PagedResult<UserDTO>> GetAllUsersAsync(
        string role,
        string search,
        int page,
        int pageSize
    )
    {
        try
        {
            // Validate inputs
            if (page < 1)
            {
                _logger.LogWarning("Số trang không hợp lệ: {Page}. Đặt mặc định là 1.", page);
                page = 1;
            }
            if (pageSize < 1)
            {
                _logger.LogWarning(
                    "Kích thước trang không hợp lệ: {PageSize}. Đặt mặc định là 10.",
                    pageSize
                );
                pageSize = 10;
            }

            // Check if role exists (if provided)
            if (!string.IsNullOrEmpty(role))
            {
                bool roleExists = await _roleRepository.RoleExistsAsync(role);
                if (!roleExists)
                {
                    _logger.LogWarning("Vai trò '{Role}' không tồn tại.", role);
                    throw new ArgumentException($"Vai trò '{role}' không tồn tại.");
                }
            }

            // Fetch users with pagination
            var query = _userRepository
                .GetAll()
                .AsNoTracking()
                .Select(u => new
                {
                    User = u,
                    Roles = _userRoleRepository
                        .GetAll()
                        .Where(ur => ur.UserId == u.UserId)
                        .Join(
                            _roleRepository.GetAll(),
                            ur => ur.RoleId,
                            r => r.RoleId,
                            (ur, r) => r.RoleName
                        )
                        .ToList(),
                    MedicalStaff = _medicalStaffRepository
                        .GetAll()
                        .Where(ms => ms.StaffId == u.UserId)
                        .FirstOrDefault(),
                    Patient = _patientRepository
                        .GetAll()
                        .Where(p => p.PatientId == u.UserId)
                        .FirstOrDefault(),
                });

            // Apply filters only if provided
            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(x => x.Roles.Contains(role));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    (
                        x.User.FullName != null
                        && x.User.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    )
                    || (
                        x.User.Email != null
                        && x.User.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                    )
                );
            }

            // Get total count
            var totalItems = await query.CountAsync();

            // Fetch users with pagination
            var users = await query
                .OrderBy(x => x.User.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserDTO
                {
                    UserId = x.User.UserId,
                    Username = x.User.Username ?? string.Empty,
                    FullName = x.User.FullName ?? string.Empty,
                    Email = x.User.Email ?? string.Empty,
                    Phone = x.User.Phone,
                    Gender = x.User.Gender,
                    Address = x.User.Address,
                    DateOfBirth = x.User.DateOfBirth,
                    IsActive = x.User.IsActive,
                    Roles = x.Roles.Any() ? x.Roles : new List<string>(),
                    Specialty = x.MedicalStaff != null ? x.MedicalStaff.Specialty : null,
                    LicenseNumber = x.MedicalStaff != null ? x.MedicalStaff.LicenseNumber : null,
                })
                .ToListAsync();
            _logger.LogInformation(
                "Lấy được {Count} người dùng cho trang {Page} với kích thước trang {PageSize}, vai trò: {Role}, tìm kiếm: {Search}",
                users.Count,
                page,
                pageSize,
                role ?? "none",
                search ?? "none"
            );

            return new PagedResult<UserDTO>
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = users,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi lấy danh sách người dùng với vai trò: {Role}, tìm kiếm: {Search}, trang: {Page}, kích thước trang: {PageSize}",
                role ?? "none",
                search ?? "none",
                page,
                pageSize
            );
            throw;
        }
    }

    public async Task ToggleUserActiveAsync(int userId, bool isActive)
    {
        try
        {
            _logger.LogInformation(
                "Yêu cầu cập nhật trạng thái tài khoản cho userId: {UserId}, isActive: {IsActive}",
                userId,
                isActive
            );
            //find user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Không tìm thấy người dùng với userId: {UserId}", userId);
                throw new KeyNotFoundException("User not found");
            }
            user.IsActive = isActive;
            await _userRepository.Update(user);
            //update user role
            await _uow.SaveChangesAsync();
            _logger.LogInformation(
                "Cập nhật trạng thái tài khoản thành công cho userId: {UserId}, isActive: {IsActive}",
                userId,
                isActive
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi cập nhật trạng thái tài khoản cho userId: {UserId}, isActive: {IsActive}",
                userId,
                isActive
            );
            throw;
        }
    }

    public async Task DeleteUserAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Yêu cầu xóa người dùng với userId: {UserId}", userId);
            //find user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Không tìm thấy người dùng với userId: {UserId}", userId);
                throw new KeyNotFoundException("User not found");
            }
            //delete user
            await _userRepository.DeleteAsync(userId);
            _logger.LogInformation("Xóa người dùng thành công với userId: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa người dùng với userId: {UserId}", userId);
            throw;
        }
    }
}
