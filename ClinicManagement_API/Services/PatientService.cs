using System.Text.Json;
using ClinicManagement_API.Models;
using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;
using ClinicManagementSystem.Application.DTOs.Auth;
using Microsoft.EntityFrameworkCore;

public interface IPatinetService
{
    Task<List<PatientDTO>> GetAllPatientsAsync();
    Task<PatientDTO?> GetPatientByIdAsync(int id);
    Task<ResponseValue<PatientRegisterDto>> RegisterPatientAsync(PatientRegisterDto patientDto);
    Task<ResponseValue<AppointmentDTO>> CreateAppointmentByPatientAsync(
        AppointmentDTO request,
        int patientId
    );
    Task<ResponseValue<List<TimeSlotDTO>>> GetAvailableTimeSlotsAsync(DateOnly date);
    Task<ResponseValue<List<MyAppointmentDTO>>> GetMyAppointmentAsync(int patientId);
    Task<ResponseValue<List<MedicalRecordDetailDTO>>> GetMedicalRecordByPatientAsync(int patientId);
}

public class PatinetService : IPatinetService
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _uow;
    private readonly IRoleRepository _roleRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ILogger<PatinetService> _logger;

    private readonly IMedicalRecordDetailRepository _medicalRecordDetailRepository;
    private readonly SupabaseContext _context;

    public PatinetService(
        IUnitOfWork uow,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPatientRepository patientRepository,
        IUserRoleRepository userRoleRepository,
        ILogger<PatinetService> logger,
        IMedicalRecordDetailRepository medicalRecordDetailRepository,
        IAppointmentRepository appointmentRepository,
        SupabaseContext context
    )
    {
        _uow = uow;
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _logger = logger;
        _appointmentRepository = appointmentRepository;
        _medicalRecordDetailRepository = medicalRecordDetailRepository;
        _context = context;
    }

    public async Task<List<PatientDTO>> GetAllPatientsAsync()
    {
        try
        {
            var patients = await _patientRepository.GetAllAsync();

            var patientList = patients
                .Select(p => new PatientDTO
                {
                    PatientId = p.PatientId,
                    MedicalHistory = p.MedicalHistory,
                })
                .ToList();

            return patientList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching medical record list");
            throw;
        }
    }

    public async Task<PatientDTO?> GetPatientByIdAsync(int id)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(id);

            if (patient == null)
            {
                throw new InvalidOperationException("Bệnh nhân không tồn tại.");
            }

            var patientDto = new PatientDTO
            {
                PatientId = patient.PatientId,
                MedicalHistory = patient.MedicalHistory,
            };

            return patientDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient with PatientId: {PatientId}", id);
            throw;
        }
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
                (u.Username == registerDto.PhoneNumber || u.Email == registerDto.Email)
            );
            if (existingUsers.Any(u => u.IsActive == true))
            {
                return new ResponseValue<PatientRegisterDto>(
                    null,
                    StatusReponse.BadRequest,
                    "Số điện thoại hoặc email đã tồn tại."
                );
            }
            var existingUser = existingUsers.FirstOrDefault(u => u.IsActive == false);
            if (existingUser != null)
            {
                //Kiểm tra email đã tồn tại chưa
                var emailOwner = await _userRepository.SingleOrDefaultAsync(u =>
                    u.Email == registerDto.Email
                );
                if (emailOwner != null && emailOwner.UserId != existingUser.UserId)
                {
                    await transaction.RollbackAsync();
                    return new ResponseValue<PatientRegisterDto>(
                        null,
                        StatusReponse.BadRequest,
                        "Email đã tồn tại."
                    );
                }

                existingUser.FullName = registerDto.FullName;
                existingUser.Email = registerDto.Email;
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
                existingUser.Phone = registerDto.PhoneNumber;
                existingUser.Address = registerDto.Address;
                existingUser.DateOfBirth = registerDto.DateOfBirth;
                existingUser.Gender = registerDto.Gender.ToString();
                await _userRepository.Update(existingUser);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ResponseValue<PatientRegisterDto>(
                    registerDto,
                    StatusReponse.Success,
                    "Vui lòng xác thực email để kích hoạt tài khoản."
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
                IsActive = false,
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

    public async Task<ResponseValue<AppointmentDTO>> CreateAppointmentByPatientAsync(
        AppointmentDTO request,
        int patientId
    )
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var nowTime = TimeOnly.FromDateTime(DateTime.Now);
            // 1. Không cho đặt ngày quá khứ
            if (request.AppointmentDate < today)
            {
                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Không thể tạo lịch hẹn trong quá khứ."
                );
            }

            // 2. Nếu đặt đúng hôm nay → giờ phải lớn hơn thời gian hiện tại
            if (request.AppointmentDate == today && request.AppointmentTime <= nowTime)
            {
                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Giờ hẹn không hợp lệ."
                );
            }
            //đã có lịch đặt trong ngày hôm nay
            var existingAppointment = await _appointmentRepository
                .GetAll()
                .Where(a =>
                    a.PatientId == patientId && a.AppointmentDate == request.AppointmentDate
                )
                .FirstOrDefaultAsync();
            if (existingAppointment != null)
            {
                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Bạn đã có lịch hẹn trong ngày này."
                );
            }

            using var transaction = await _uow.BeginTransactionAsync();
            int? recordId = _medicalRecordDetailRepository
                .GetAll()
                .Where(mr => mr.PatientId == patientId)
                .Select(mr => mr.RecordId)
                .FirstOrDefault();

            var appointment = new Appointment
            {
                PatientId = patientId,
                RecordId = recordId,
                AppointmentDate = request.AppointmentDate,
                AppointmentTime = request.AppointmentTime,
                Status = "Ordered",
                Notes = request.Notes,
                CreatedAt = DateTime.Now,
                CreatedBy = patientId,
            };
            await _appointmentRepository.AddAsync(appointment);
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();
            return new ResponseValue<AppointmentDTO>(
                new AppointmentDTO
                {
                    PatientId = appointment.PatientId,
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Status = appointment.Status,
                    Notes = appointment.Notes,
                    CreatedAt = appointment.CreatedAt,
                },
                StatusReponse.Success,
                "Tạo lịch hẹn thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<AppointmentDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi tạo lịch hẹn: " + ex.Message
            );
        }
    }

    //lấy lịch hẹn hợp lệ
    public async Task<ResponseValue<List<TimeSlotDTO>>> GetAvailableTimeSlotsAsync(DateOnly date)
    {
        var timeSlots = new List<TimeOnly>
        {
            // lấy từ 7am đến 16:30pm
            new(7, 0),
            new(7, 30),
            new(8, 0),
            new(8, 30),
            new(9, 0),
            new(9, 30),
            new(10, 0),
            new(10, 30),
            new(11, 0),
            new(13, 0),
            new(13, 30),
            new(14, 0),
            new(14, 30),
            new(15, 0),
            new(15, 30),
            new(16, 0),
        };
        const int MaxPerSlot = 10;

        var appointments = await _appointmentRepository
            .GetAll()
            .Where(a =>
                a.AppointmentDate == date && a.Status != "Cancelled" && a.Status != "Completed"
            )
            .ToListAsync();
        var result = timeSlots
            .Select(slot =>
            {
                var count = appointments.Count(a => a.AppointmentTime == slot);

                return new TimeSlotDTO
                {
                    Time = slot.ToString("HH:mm"),
                    BookedCount = count,
                    Available = count < MaxPerSlot,
                };
            })
            .ToList();

        var today = DateOnly.FromDateTime(DateTime.Now);
        var now = TimeOnly.FromDateTime(DateTime.Now);
        if (date == today)
        {
            result = result.Where(r => TimeOnly.Parse(r.Time) > now).ToList();
        }
        return new ResponseValue<List<TimeSlotDTO>>(
            result,
            StatusReponse.Success,
            "Lấy danh sách lịch hẹn thành công."
        );
    }

    public async Task<ResponseValue<List<MyAppointmentDTO>>> GetMyAppointmentAsync(int patientId)
    {
        try
        {
            var appointments = await _appointmentRepository
                .GetAll()
                .Where(q => q.PatientId == patientId)
                .AsNoTracking()
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new MyAppointmentDTO
                {
                    AppointmentId = a.AppointmentId,
                    StaffName = a.Staff != null ? a.Staff.FullName : "Không rõ",
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status,
                    Notes = a.Notes,
                })
                .ToListAsync();
            return new ResponseValue<List<MyAppointmentDTO>>(
                appointments,
                StatusReponse.Success,
                "Lấy danh sách lịch hẹn thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<List<MyAppointmentDTO>>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy danh sách lịch hẹn: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<List<MedicalRecordDetailDTO>>> GetMedicalRecordByPatientAsync(
        int patientId
    )
    {
        try
        {
            var rawRecords = await _medicalRecordDetailRepository
                .GetAll()
                .Where(mr => mr.PatientId == patientId)
                .Select(mr => new
                {
                    mr.RecordId,
                    mr.RecordNumber,
                    mr.IssuedDate,
                    mr.PatientId,
                    mr.PatientName,
                    mr.RecordStatus,
                    mr.Appointments,
                })
                .ToListAsync();

            // THÊM CONVERTER VÀO ĐÂY
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            jsonOptions.Converters.Add(new DateOnlyJsonConverter());

            var result = rawRecords
                .Select(r => new MedicalRecordDetailDTO
                {
                    RecordId = r.RecordId,
                    RecordNumber = r.RecordNumber,
                    IssuedDate = r.IssuedDate,
                    PatientId = r.PatientId,
                    PatientName = r.PatientName,
                    RecordStatus = r.RecordStatus,

                    Appointments =
                        !string.IsNullOrWhiteSpace(r.Appointments) && r.Appointments != "[]"
                            ? JsonSerializer.Deserialize<List<AppointmentForPatientDTO>>(
                                r.Appointments,
                                jsonOptions
                            ) ?? new()
                            : new(),
                })
                .ToList();

            return new ResponseValue<List<MedicalRecordDetailDTO>>(
                result,
                StatusReponse.Success,
                "Lấy danh sách hồ sơ bệnh án thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<List<MedicalRecordDetailDTO>>(
                null,
                StatusReponse.Error,
                "Lỗi: " + ex.Message
            );
        }
    }
}

public class TimeSlotDTO
{
    public string Time { get; set; }
    public int BookedCount { get; set; }
    public bool Available { get; set; }
}

public class MyAppointmentDTO
{
    public int AppointmentId { get; set; }
    public string StaffName { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public string Status { get; set; }
    public string Notes { get; set; }
}
