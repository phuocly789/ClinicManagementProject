using ClinicManagement_Infrastructure.Data.Models;

public interface IAppointmentService
{
    Task<List<AppointmentDTO>> GetAllAppointmentsAsync();
    Task<List<AppointmentMyScheduleDto>> GetAppointmentsAsync(int staffId, DateOnly? date = null);
    Task<ResponseValue<AppointmentDTO>> AddToAppointmentAsync(AppointmentCreateDTO request, int createdBy);
    Task<ResponseValue<AppointmentDTO>> AppointmentUpdateStatusAsync(int appointmentId, AppointmentStatusDTO request);
    Task<ResponseValue<AppointmentDTO>> AppointmentCancelAsync(int appointmentId, int patientId);
}

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IStaffScheduleRepository _staffScheduleRepository;
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly ILogger<AppointmentService> _logger;
    private readonly IUnitOfWork _uow;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IStaffScheduleRepository staffScheduleRepository,
        IMedicalRecordRepository medicalRecordRepository,
        ILogger<AppointmentService> logger,
        IUnitOfWork uow
    )
    {
        _appointmentRepository = appointmentRepository;
        _staffScheduleRepository = staffScheduleRepository;
        _medicalRecordRepository = medicalRecordRepository;
        _logger = logger;
        _uow = uow;
    }

    public async Task<List<AppointmentDTO>> GetAllAppointmentsAsync()
    {
        try
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsAsync();

            var appointmentList = appointments.Select(a => new AppointmentDTO
            {
                AppointmentId = a.AppointmentId,
                PatientId = a.PatientId,
                PatientName = a.Patient?.FullName,
                StaffId = a.StaffId,
                StaffName =  a.Staff?.FullName,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                Status = a.Status,
                Notes = a.Notes
            }).ToList();

            return appointmentList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointment list");
            throw;
        }
    }

    public async Task<List<AppointmentMyScheduleDto>> GetAppointmentsAsync(int staffId, DateOnly? date = null)
    {
        try
        {
            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Now);

            return await _appointmentRepository.GetAppointmentsByStaffIdAnddDateAsync(staffId, selectedDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointment for staff {StaffId}", staffId);
            throw;
        }
    }

    public async Task<ResponseValue<AppointmentDTO>> AddToAppointmentAsync(AppointmentCreateDTO request, int createdBy)
    {
        try
        {
            if (!await _staffScheduleRepository.HasScheduleAsync(request.StaffId, request.AppointmentDate))
            {
                throw new InvalidOperationException("Bác sĩ không có lịch làm việc.");
            }

            if (!await _appointmentRepository.IsDoctorAvailableAsync(request.StaffId, request.AppointmentDate, request.AppointmentTime))
            {
                throw new InvalidOperationException("Bác sĩ không khả dụng.");
            }

            var record = await _medicalRecordRepository.GetByPatientIdAsync(request.PatientId);
            {
                if (record == null)
                {
                    throw new ArgumentException("Dữ liệu cuộc hẹn không hợp lệ.");
                }
            }

            using var transaction = await _uow.BeginTransactionAsync();

            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                StaffId = request.StaffId,
                AppointmentDate = request.AppointmentDate,
                AppointmentTime = request.AppointmentTime,
                Notes = request.Notes,
                Status = "Waiting",
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now
            };

            await _appointmentRepository.AddAsync(appointment);
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResponseValue<AppointmentDTO>(
                new AppointmentDTO
                {
                    AppointmentId = appointment.AppointmentId,
                    PatientId = appointment.PatientId,
                    StaffId = appointment.StaffId,
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime
                },
                StatusReponse.Success,
                "Tạo lịch hẹn thành công."
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while adding to appoitment: {@Request}", request);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding to appointment: {@Request}", request);
            return new ResponseValue<AppointmentDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<AppointmentDTO>> AppointmentUpdateStatusAsync(int appointmentId, AppointmentStatusDTO request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thông tin trạng thái không hợp lệ."
                );
            }

            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.NotFound,
                    "Không tìm thấy lịch hẹn."
                );
            }

            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                appointment.Status = request.Status;

                await _appointmentRepository.Update(appointment);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.Success,
                    "Cập nhật trạng thái thành công."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi cập nhật trạng thái: " + ex.Message
                );
            }
        }
        catch (Exception ex)
        {
            return new ResponseValue<AppointmentDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi cập nhật trạng thái: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<AppointmentDTO>> AppointmentCancelAsync(int appointmentId, int patientId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);

            if (appointment == null)
            {
                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Không tìm thấy lịch hẹn."
                );
            }

            if (appointment.PatientId != patientId)
            {
                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.Unauthorized,
                    "Không thể hủy lịch hẹn của người khác.");
            }

            var diff = appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue) - DateTime.Now;
            var hoursUntil = diff.TotalHours;

            if (hoursUntil < 24)
            {
                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.Error,
                    "Không thể hủy lịch hẹn trong vòng 24 giờ"
                );
            }

            using var transaction = await _uow.BeginTransactionAsync();

            try
            {
                appointment.Status = "Hủy";

                await _appointmentRepository.Update(appointment);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.Success,
                    "Hủy lịch hẹn thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseValue<AppointmentDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi hủy lịch hẹn."
                );
            }
        }
        catch (Exception ex)
        {
            return new ResponseValue<AppointmentDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi hủy lịch hẹn: " + ex.Message
            );
        }
    }
}