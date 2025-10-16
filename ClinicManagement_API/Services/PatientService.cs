using ClinicManagement_API.Models;
using ClinicManagement_Infrastructure.Data.Models;
using ClinicManagementSystem.Application.DTOs.Auth;

public interface IPatinetService
{
    Task<ResponseValue<PatientRegisterDto>> RegisterPatientAsync(PatientRegisterDto patientDto);
}

public class PatinetService : IPatinetService
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _uow;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public PatinetService(
        IUnitOfWork uow,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPatientRepository patientRepository,
        IUserRoleRepository userRoleRepository
    )
    {
        _uow = uow;
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
    }

    public async Task<ResponseValue<PatientRegisterDto>> RegisterPatientAsync(
        PatientRegisterDto registerDto
    )
    {
        // Kiểm tra mật khẩu xác nhận
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return new ResponseValue<PatientRegisterDto>(
                null,
                StatusReponse.BadRequest,
                "Mật khẩu xác nhận không khớp."
            );
        }

        await using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            // Kiểm tra SĐT hoặc Email đã tồn tại chưa
            var existingUsers = await _userRepository.WhereAsync(u =>
                u.Username == registerDto.PhoneNumber || u.Email == registerDto.Email
            );
            if (existingUsers.Any())
            {
                return new ResponseValue<PatientRegisterDto>(
                    null,
                    StatusReponse.BadRequest,
                    "Số điện thoại hoặc email đã tồn tại."
                );
            }

            // Tạo User mới
            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                Phone = registerDto.PhoneNumber,
                Username = registerDto.PhoneNumber, // Vẫn dùng SĐT làm username để đăng nhập
                DateOfBirth = registerDto.DateOfBirth,
                Gender = registerDto.Gender.ToString(),
                Address = registerDto.Address,
                // Mã hóa mật khẩu do người dùng cung cấp
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                // Không bắt buộc đổi mật khẩu vì người dùng đã tự đặt
                MustChangePassword = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            await _userRepository.AddAsync(user);
            await _uow.SaveChangesAsync(); // Lưu để lấy UserId

            // Tạo Patient record liên kết với User
            var patient = new Patient
            {
                PatientId = user.UserId,
                // Không tạo MedicalRecord ngay lúc đăng ký, sẽ tạo khi có lịch hẹn đầu tiên.
                MedicalRecords = new List<MedicalRecord>(),
            };
            await _patientRepository.AddAsync(patient);

            // Gán vai trò "Patient"
            var patientRole = await _roleRepository.SingleOrDefaultAsync(r =>
                r.RoleName == "Patient"
            );
            if (patientRole == null)
            {
                await transaction.RollbackAsync();
                return new ResponseValue<PatientRegisterDto>(
                    null,
                    StatusReponse.BadRequest,
                    "Vai trò bệnh nhân không tồn tại trong hệ thống."
                );
            }
            var userRole = new UserRole { UserId = user.UserId, RoleId = patientRole.RoleId };
            await _userRoleRepository.AddAsync(userRole);

            // Lưu tất cả thay đổi và hoàn tất transaction
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResponseValue<PatientRegisterDto>(
                registerDto,
                StatusReponse.Success,
                "Đăng ký tài khoản thành công."
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResponseValue<PatientRegisterDto>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi đăng ký: " + ex.Message
            );
        }
    }
}
