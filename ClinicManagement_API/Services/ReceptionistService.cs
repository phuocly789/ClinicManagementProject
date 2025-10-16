using ClinicManagement_Infrastructure.Data.Models;
using ClinicManagementSystem.Application.DTOs.Auth;

public interface IReceptionistService
{
    Task<ResponseValue<CreatePatientByReceptionistDto>> CreatePatientAndAccountAsync(
        CreatePatientByReceptionistDto patientDto
    );
    Task<bool> ResetPasswordAsync(int patientId);
}

public class ReceptionistService : IReceptionistService
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _uow;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    private string GenerateMedicalRecordNumber()
    {
        // Tạo mã hồ sơ bệnh án theo format riêng
        // Ví dụ: MR-20250914-12345
        return $"MR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 5)}";
    }

    public ReceptionistService(
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

    public async Task<ResponseValue<CreatePatientByReceptionistDto>> CreatePatientAndAccountAsync(
        CreatePatientByReceptionistDto patientDto
    )
    {
        await using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            var existingUsers = await _userRepository.WhereAsync(u =>
                u.Username == patientDto.PhoneNumber || u.Email == patientDto.Email
            );
            if (existingUsers.Any())
            {
                return new ResponseValue<CreatePatientByReceptionistDto>(
                    null,
                    StatusReponse.BadRequest,
                    "Số điện thoại hoặc email đã tồn tại."
                );
            }
            var user = new User
            {
                FullName = patientDto.FullName,
                Email = patientDto.Email,
                Phone = patientDto.PhoneNumber,
                Username = patientDto.PhoneNumber, // Dùng SĐT làm username
                DateOfBirth = patientDto.DateOfBirth,
                Gender = patientDto.Gender.ToString(),
                Address = patientDto.Address,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(patientDto.PhoneNumber), // Mã hóa SĐT làm mật khẩu
                MustChangePassword = true, // Bắt buộc đổi mật khẩu khi đăng nhập lần đầu
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            await _userRepository.AddAsync(user);
            await _uow.SaveChangesAsync();

            var patient = new Patient
            {
                PatientId = user.UserId,
                MedicalRecords = new List<MedicalRecord>
                {
                    new MedicalRecord { RecordNumber = GenerateMedicalRecordNumber() },
                },
                MedicalHistory = patientDto.MedicalHistory,
            };
            await _patientRepository.AddAsync(patient);

            var patientRole = await _roleRepository.SingleOrDefaultAsync(r =>
                r.RoleName == "Patient"
            );
            if (patientRole == null)
            {
                await transaction.RollbackAsync();
                return new ResponseValue<CreatePatientByReceptionistDto>(
                    null,
                    StatusReponse.BadRequest,
                    "Vai trò bệnh nhân không tồn tại trong hệ thống."
                );
            }
            var userRole = new UserRole { UserId = user.UserId, RoleId = patientRole.RoleId };
            await _userRoleRepository.AddAsync(userRole);

            //lưu tất cả thay đổi
            await _uow.SaveChangesAsync();
            //hoàn tất transaction
            await transaction.CommitAsync();

            return new ResponseValue<CreatePatientByReceptionistDto>(
                patientDto,
                StatusReponse.Success,
                "Tạo bệnh nhân và tài khoản thành công."
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResponseValue<CreatePatientByReceptionistDto>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi tạo bệnh nhân: " + ex.Message
            );
        }
    }

    public async Task<bool> ResetPasswordAsync(int patientId)
    {
        var user = await _userRepository.SingleOrDefaultAsync(u => u.UserId == patientId);
        if (user == null)
        {
            return false; // Người dùng không tồn tại
        }

        // Reset mật khẩu về số điện thoại
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Phone);
        user.MustChangePassword = true; // Bắt buộc đổi mật khẩu khi đăng nhập lần tiếp theo

        await _userRepository.Update(user);
        await _uow.SaveChangesAsync();

        return true;
    }
}
