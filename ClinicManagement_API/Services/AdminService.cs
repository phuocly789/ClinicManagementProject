using ClinicManagement_API.Models;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using ClinicManagementAPI.Models;
using dotnet03WebApi_EbayProject.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public interface IAdminService
{
    Task<ResponseValue<PagedResult<UserDTO>>> GetAllUsersAsync(
        string role,
        string search,
        int page,
        int pageSize
    );
    Task<ResponseValue<CreateUserResponse>> CreateUserAsync(CreateUserRequest request);
    Task<ResponseValue<UpdateUserResponse>> UpdateUserAsync(int userId, UpdateUserRequest request);
    Task<ResponseValue<ResetPasswordResponse>> ResetPasswordAsync(int userId);
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

    public async Task<ResponseValue<PagedResult<UserDTO>>> GetAllUsersAsync(
        string role,
        string search,
        int page,
        int pageSize
    )
    {
        try
        {
            if (page < 1)
            {
                page = 1;
            }
            if (pageSize < 1)
            {
                pageSize = 10;
            }

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
                });

            // Log chi tiết để debug
            var tempQuery = await query.ToListAsync();
            foreach (var x in tempQuery)
            {
                _logger.LogInformation(
                    "UserId: {UserId}, Username: {Username}, MedicalStaff: {MedicalStaff}, StaffType: {StaffType}",
                    x.User.UserId,
                    x.User.Username,
                    x.MedicalStaff != null ? "Found" : "Not Found",
                    x.MedicalStaff?.StaffType ?? "null"
                );
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(x => x.Roles.Contains(role));
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    (x.User.FullName != null && EF.Functions.Like(x.User.FullName, $"%{search}%"))
                    || (x.User.Email != null && EF.Functions.Like(x.User.Email, $"%{search}%"))
                );
            }

            var totalItems = await query.CountAsync();

            var users = await query
                .OrderByDescending(x => x.User.IsActive)
                .ThenBy(x => x.User.UserId)
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
                    Bio = x.MedicalStaff != null ? x.MedicalStaff.Bio : null,
                    StaffType = x.MedicalStaff != null ? x.MedicalStaff.StaffType : null,
                })
                .ToListAsync();

            _logger.LogInformation(
                "Fetched {Count} users for page {Page}, role: {Role}, search: {Search}",
                users.Count,
                page,
                role ?? "none",
                search ?? "none"
            );

            return new ResponseValue<PagedResult<UserDTO>>(
                new PagedResult<UserDTO>
                {
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    Items = users,
                },
                StatusReponse.Success,
                "Lấy danh sách người dùng thành công."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error fetching users: role={Role}, search={Search}, page={Page}, pageSize={PageSize}",
                role ?? "none",
                search ?? "none",
                page,
                pageSize
            );
            throw;
        }
    }

    public async Task<ResponseValue<CreateUserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        //kiểm tra trung lặp email và sdt
        if (
            await _userRepository
                .GetAll()
                .AnyAsync(u => u.Email == request.Email || u.Phone == request.Phone)
        )
        {
            return new ResponseValue<CreateUserResponse>(
                null,
                StatusReponse.BadRequest,
                "Email hoặc số điện thoại đã tồn tại."
            );
        }
        if (await _userRepository.GetAll().AnyAsync(u => u.Username == request.Username))
        {
            return new ResponseValue<CreateUserResponse>(
                null,
                StatusReponse.BadRequest,
                "Username đã tồn tại."
            );
        }
        if (!await _roleRepository.GetAll().AnyAsync(r => r.RoleId == request.RoleId))
        {
            return new ResponseValue<CreateUserResponse>(
                null,
                StatusReponse.BadRequest,
                "Vai trò không tồn tại."
            );
        }
        //mã hóa mật khẩu
        string hashedPassword;
        try
        {
            hashedPassword = PasswordHelper.HashPassword(request.Phone);
        }
        catch (ArgumentException ex)
        {
            return new ResponseValue<CreateUserResponse>(
                null,
                StatusReponse.BadRequest,
                ex.Message
            );
        }
        using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            //tạo user mới
            var user = new User1
            {
                Username = request.Username,
                PasswordHash = hashedPassword,
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                MustChangePassword = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            await _userRepository.AddAsync(user);
            await _uow.SaveChangesAsync();
            //gán role
            var UserRole = new UserRole { RoleId = request.RoleId, UserId = user.UserId };
            await _userRoleRepository.AddAsync(UserRole);

            //nếu là medical staff
            if (!string.IsNullOrEmpty(request.StaffType))
            {
                var medicalStaff = new MedicalStaff
                {
                    StaffId = user.UserId,
                    StaffType = request.StaffType,
                    Specialty = request.Specialty,
                    LicenseNumber = request.LicenseNumber,
                    Bio = request.Bio,
                };
                await _medicalStaffRepository.AddAsync(medicalStaff);
            }
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();
            return new ResponseValue<CreateUserResponse>(
                new CreateUserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                },
                StatusReponse.Success,
                "Tạo tài khoản thành công."
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResponseValue<CreateUserResponse>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi tạo tài khoản: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<UpdateUserResponse>> UpdateUserAsync(
        int userId,
        UpdateUserRequest request
    )
    {
        var user = await _userRepository
            .GetAll()
            .Include(u => u.UserRoles)
            .Include(u => u.MedicalStaff)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return new ResponseValue<UpdateUserResponse>(
                null,
                StatusReponse.NotFound,
                "Không tìm thấy user"
            );
        }
        //kiểm tra email
        if (
            await _userRepository
                .GetAll()
                .AnyAsync(u => u.Email == request.Email && u.UserId != userId)
        )
        {
            return new ResponseValue<UpdateUserResponse>(
                null,
                StatusReponse.BadRequest,
                "Email đã tồn tại"
            );
        }
        if (!await _roleRepository.GetAll().AnyAsync(r => r.RoleId == request.RoleId))
        {
            return new ResponseValue<UpdateUserResponse>(
                null,
                StatusReponse.BadRequest,
                "Vai trò không tồn tại."
            );
        }
        using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.Gender = request.Gender;
            user.Address = request.Address;
            user.DateOfBirth = request.DateOfBirth;
            var currentRole = user.UserRoles?.FirstOrDefault();
            if (currentRole == null || currentRole.RoleId != request.RoleId)
            {
                if (currentRole != null)
                {
                    var entity = await _userRoleRepository
                        .GetAll()
                        .FirstOrDefaultAsync(ur =>
                            ur.UserId == user.UserId && ur.RoleId == currentRole.RoleId
                        );
                    if (entity != null)
                    {
                        await _userRoleRepository.DeleteAsync(entity);
                    }
                }
                var newUserRole = new UserRole
                {
                    UserId = user.UserId,
                    RoleId = request.RoleId,
                    AssignedAt = DateTime.UtcNow,
                };
                await _userRoleRepository.AddAsync(newUserRole);
            }
            if (!string.IsNullOrEmpty(request.StaffType))
            {
                if (user.MedicalStaff == null)
                {
                    var medicalStaff = new MedicalStaff
                    {
                        StaffId = user.UserId,
                        StaffType = request.StaffType,
                        Specialty = request.Specialty,
                        LicenseNumber = request.LicenseNumber,
                        Bio = request.Bio,
                    };
                    await _medicalStaffRepository.AddAsync(medicalStaff);
                }
                else
                {
                    user.MedicalStaff.StaffType = request.StaffType;
                    user.MedicalStaff.Specialty = request.Specialty;
                    user.MedicalStaff.LicenseNumber = request.LicenseNumber;
                    user.MedicalStaff.Bio = request.Bio;
                }
            }
            else
            {
                if (user.MedicalStaff != null)
                {
                    await _medicalStaffRepository.DeleteAsync(user.MedicalStaff.StaffId);
                }
            }
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();
            return new ResponseValue<UpdateUserResponse>(
                new UpdateUserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                },
                StatusReponse.Success,
                "Cập nhật tài khoản thành công."
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResponseValue<UpdateUserResponse>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi cập nhật tài khoản: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<ResetPasswordResponse>> ResetPasswordAsync(int userId)
    {
        //tìm user
        var user = await _userRepository.GetAll().FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return new ResponseValue<ResetPasswordResponse>(
                null,
                StatusReponse.NotFound,
                "Không tìm thấy user"
            );
        }
        //kiểm tra sdt
        if (string.IsNullOrEmpty(user.Phone))
        {
            return new ResponseValue<ResetPasswordResponse>(
                null,
                StatusReponse.BadRequest,
                "Số điện thoại không hợp lệ"
            );
        }
        //làm lại mk
        string hashedPassword;
        try
        {
            hashedPassword = PasswordHelper.HashPassword(user.Phone);
        }
        catch (ArgumentException ex)
        {
            return new ResponseValue<ResetPasswordResponse>(
                null,
                StatusReponse.BadRequest,
                ex.Message
            );
        }
        using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            //cập nhật passwordHash và reset
            user.PasswordHash = hashedPassword;
            user.MustChangePassword = true;
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();
            return new ResponseValue<ResetPasswordResponse>(
                new ResetPasswordResponse { Message = "Đặt lại mật khẩu thành công." },
                StatusReponse.Success,
                "Đặt lại mật khẩu thành công."
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResponseValue<ResetPasswordResponse>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi đặt lại mật khẩu: " + ex.Message
            );
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
            await _uow.SaveChangesAsync();
            _logger.LogInformation("Xóa người dùng thành công với userId: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa người dùng với userId: {UserId}", userId);
            throw;
        }
    }
}
