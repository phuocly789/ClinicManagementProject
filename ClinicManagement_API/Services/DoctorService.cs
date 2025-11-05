using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public interface IDoctorService
{
    Task<int?> GetCurrentRoomId(int staffId);
    Task<ResponseValue<List<ScheduleForMedicalStaffResponse>>> GetAllMySchedulesAsync(int staffId);

    Task<List<TodaysAppointmentDTO>> GetTodaysAppointmentsAsync(DateOnly date);

    // ----------------------------------------
    Task<ResponseValue<object>> SubmitExaminationAsync(ExaminationRequestDto request, int staffId);
    Task<ResponseValue<PagedResult<MedicineDTO>>> GetAllMedicinesAsync(string? search);
    Task<ResponseValue<PagedResult<ServiceDTO>>> GetAllServicesAsync(string? search);

    // ----------------------------------------

    //lấy danh sách hàng chờ của mình
    Task<List<DoctorQueueDto>> GetMyQueueTodayAsync(int doctorId, DateOnly? date = null);
}

public class DoctorService : IDoctorService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDiagnosisRepository _diagnosisRepository;
    private readonly IUnitOfWork _uow;
    private readonly IServiceService _serviceService;
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMedicalStaffRepository _medicalStaffRepository;
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IPrescriptionDetailRepository _prescriptionDetailRepository;
    private readonly IMedicineRepository _medicineRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IStaffScheduleRepository _staffScheduleRepository;
    private readonly IHubContext<QueueHub> _hubContext;
    private readonly IQueueRepository _queueRepository;

    public DoctorService(
        IAppointmentRepository appointmentRepository,
        IDiagnosisRepository diagnosisRepository,
        IUnitOfWork uow,
        IServiceService serviceService,
        IUserRepository userRepository,
        IServiceOrderRepository serviceOrderRepository,
        IMedicalStaffRepository medicalStaffRepository,
        IPrescriptionRepository prescriptionRepository,
        IPrescriptionDetailRepository prescriptionDetailRepository,
        IMedicineRepository medicineRepository,
        IServiceRepository serviceRepository,
        IStaffScheduleRepository staffScheduleRepository,
        IQueueRepository queueRepository,
        IHubContext<QueueHub> hubContext
    )
    {
        _appointmentRepository = appointmentRepository;
        _diagnosisRepository = diagnosisRepository;
        _serviceService = serviceService;
        _userRepository = userRepository;
        _serviceOrderRepository = serviceOrderRepository;
        _uow = uow;
        _medicalStaffRepository = medicalStaffRepository;
        _prescriptionRepository = prescriptionRepository;
        _prescriptionDetailRepository = prescriptionDetailRepository;
        _medicineRepository = medicineRepository;
        _serviceRepository = serviceRepository;
        _staffScheduleRepository = staffScheduleRepository;
        _hubContext = hubContext;
        _queueRepository = queueRepository;
    }

    public async Task<int?> GetCurrentRoomId(int staffId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var now = TimeOnly.FromDateTime(DateTime.Now);

        return await _uow.GetDbContext()
            .StaffSchedules.Where(s => s.StaffId == staffId && s.WorkDate == today)
            .Where(s => s.WorkDate == today)
            .Select(s => s.RoomId)
            .FirstOrDefaultAsync();
    }

    public async Task<ResponseValue<List<ScheduleForMedicalStaffResponse>>> GetAllMySchedulesAsync(
        int staffId
    )
    {
        try
        {
            var query =
                from schedule in _staffScheduleRepository.GetAll()
                join user in _userRepository.GetAll()
                    on schedule.StaffId equals user.UserId
                    into staffUsers
                from user in staffUsers.DefaultIfEmpty()
                where schedule.StaffId == staffId
                select new { schedule, user };

            var schedules = await query
                .OrderBy(q => q.schedule.ScheduleId)
                .Select(q => new ScheduleForMedicalStaffResponse
                {
                    ScheduleId = q.schedule.ScheduleId,
                    StaffId = q.schedule.StaffId,
                    StaffName = q.user != null ? q.user.FullName : "(Không xác định)",
                    Role = q.user.UserRoles.Select(r => r.Role.RoleName).FirstOrDefault(),
                    RoomId = q.schedule.RoomId,
                    WorkDate = q.schedule.WorkDate.ToString("yyyy-MM-dd"),
                    StartTime = q.schedule.StartTime.ToString("HH:mm:ss"),
                    EndTime = q.schedule.EndTime.ToString("HH:mm:ss"),
                    IsAvailable = q.schedule.IsAvailable,
                })
                .ToListAsync();

            return new ResponseValue<List<ScheduleForMedicalStaffResponse>>(
                schedules,
                StatusReponse.Success,
                "Lấy danh sách lịch thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<List<ScheduleForMedicalStaffResponse>>(
                null,
                StatusReponse.Error,
                ex.Message
            );
        }
    }

    //for dashboard
    public async Task<List<TodaysAppointmentDTO>> GetTodaysAppointmentsAsync(DateOnly date)
    {
        return await _appointmentRepository
            .GetAll()
            .Where(a => a.AppointmentDate == date)
            .Include(a => a.Patient)
            .Include(a => a.Staff)
            .OrderBy(a => a.AppointmentTime)
            .Select(a => new TodaysAppointmentDTO
            {
                AppointmentId = a.AppointmentId,
                AppointmentTime = a.AppointmentTime,
                PatientName = a.Patient.FullName,
                DoctorName = a.Staff.FullName,
                Status = a.Status,
            })
            .ToListAsync();
    }

    public async Task<ResponseValue<object>> SubmitExaminationAsync(
        ExaminationRequestDto request,
        int staffId
    )
    {
        // Bắt đầu transaction
        await using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            // 1) Lấy queue
            var queue = await _queueRepository.GetByIdAsync(request.QueueId);
            if (queue == null)
                return new ResponseValue<object>(
                    null,
                    StatusReponse.NotFound,
                    "Không tìm thấy hàng chờ."
                );

            if (queue.AppointmentId == null)
                return new ResponseValue<object>(
                    null,
                    StatusReponse.NotFound,
                    "Hàng chờ không gắn với cuộc hẹn."
                );

            // 2) Lấy Appointment thuộc đúng bác sĩ
            var appointment = await _appointmentRepository
                .GetAll()
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == queue.AppointmentId && a.StaffId == staffId
                );

            if (appointment == null)
                return new ResponseValue<object>(
                    null,
                    StatusReponse.NotFound,
                    "Cuộc hẹn không thuộc bác sĩ này."
                );

            // 3) Xử lý Chẩn đoán
            var diagnosis = await _diagnosisRepository
                .GetAll()
                .FirstOrDefaultAsync(d => d.AppointmentId == appointment.AppointmentId);

            if (diagnosis == null)
            {
                diagnosis = new Diagnosis
                {
                    AppointmentId = appointment.AppointmentId,
                    RecordId = appointment.RecordId,
                    StaffId = staffId,
                    DiagnosisDate = DateTime.UtcNow,
                };
                await _diagnosisRepository.AddAsync(diagnosis);
            }

            diagnosis.Symptoms = request.Symptoms;
            diagnosis.Diagnosis1 = request.Diagnosis;
            await _uow.SaveChangesAsync();

            // 4) Xử lý Đơn thuốc
            var prescription = await _prescriptionRepository
                .GetAll()
                .FirstOrDefaultAsync(p => p.AppointmentId == appointment.AppointmentId);

            if (prescription != null)
            {
                var oldDetails = await _prescriptionDetailRepository
                    .GetAll()
                    .Where(pd => pd.PrescriptionId == prescription.PrescriptionId)
                    .ToListAsync();

                if (oldDetails.Any())
                {
                    _prescriptionDetailRepository.RemoveRange(oldDetails);
                    await _uow.SaveChangesAsync();
                }
            }

            if (request.Prescriptions != null && request.Prescriptions.Any())
            {
                if (prescription == null)
                {
                    prescription = new Prescription
                    {
                        AppointmentId = appointment.AppointmentId,
                        RecordId = appointment.RecordId,
                        PrescriptionDate = DateTime.UtcNow,
                        StaffId = staffId,
                    };
                    await _prescriptionRepository.AddAsync(prescription);
                    await _uow.SaveChangesAsync();
                }

                foreach (var p in request.Prescriptions)
                {
                    if (
                        !await _prescriptionRepository.HasEnoughStockAsync(p.MedicineId, p.Quantity)
                    )
                        throw new Exception($"Thuốc ID {p.MedicineId} không đủ tồn kho.");

                    await _prescriptionDetailRepository.AddAsync(
                        new PrescriptionDetail
                        {
                            PrescriptionId = prescription.PrescriptionId,
                            MedicineId = p.MedicineId,
                            Quantity = p.Quantity,
                            DosageInstruction = p.DosageInstruction,
                        }
                    );

                    await _prescriptionRepository.UpdateMedicineStockAsync(
                        p.MedicineId,
                        p.Quantity
                    );
                }
            }

            // 5) Xử lý dịch vụ chỉ định
            var oldServices = await _serviceOrderRepository
                .GetAll()
                .Where(s => s.AppointmentId == appointment.AppointmentId)
                .ToListAsync();

            if (oldServices.Any())
            {
                _serviceOrderRepository.RemoveRange(oldServices);
            }

            if (request.ServiceIds != null && request.ServiceIds.Any())
            {
                foreach (var id in request.ServiceIds)
                {
                    await _serviceOrderRepository.AddAsync(
                        new ServiceOrder
                        {
                            AppointmentId = appointment.AppointmentId,
                            ServiceId = id,
                            OrderDate = DateTime.UtcNow,
                            Status = "Pending",
                        }
                    );
                }
            }

            // 6) Cập nhật trạng thái
            if (request.IsComplete)
            {
                queue.Status = "Completed";
                appointment.Status = "Completed";
            }
            else
            {
                queue.Status = "InProgress";
                appointment.Status = "InProgress";
            }

            await _queueRepository.Update(queue);
            await _appointmentRepository.Update(appointment);

            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResponseValue<object>(
                null,
                StatusReponse.Success,
                request.IsComplete ? "Hoàn tất khám bệnh thành công" : "Tạm lưu thành công"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResponseValue<object>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi hệ thống: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<PagedResult<MedicineDTO>>> GetAllMedicinesAsync(
        string? search = null
    )
    {
        try
        {
            var query = _medicineRepository.GetAll().AsNoTracking();
            // search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.MedicineName != null && EF.Functions.Like(s.MedicineName, $"%{search}%")
                );
            }

            //get total count
            var totalItems = await query.CountAsync();

            //fetch services with pagination
            var medicines = await query
                .OrderBy(m => m.MedicineId)
                .Select(m => new MedicineDTO
                {
                    MedicineId = m.MedicineId,
                    MedicineName = m.MedicineName,
                    MedicineType = m.MedicineType,
                    Unit = m.Unit,
                    Price = m.Price,
                    StockQuantity = m.StockQuantity,
                    Description = m.Description,
                })
                .ToListAsync();
            return new ResponseValue<PagedResult<MedicineDTO>>(
                new PagedResult<MedicineDTO>
                {
                    TotalItems = totalItems,
                    Page = 0,
                    PageSize = 0,
                    Items = medicines,
                },
                StatusReponse.Success,
                "Lấy danh sách thuốc thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<PagedResult<MedicineDTO>>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy danh sách thuốc: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<PagedResult<ServiceDTO>>> GetAllServicesAsync(string? search)
    {
        try
        {
            var query = _serviceRepository.GetAll().AsNoTracking();
            //search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.ServiceName != null && EF.Functions.Like(s.ServiceName, $"%{search}%")
                );
            }

            //get total count
            var totalItems = await query.CountAsync();
            //fetch services with pagination
            var services = await query
                .OrderBy(s => s.ServiceId)
                .Select(s => new ServiceDTO
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    ServiceType = s.ServiceType,
                    Price = s.Price,
                    Description = s.Description,
                })
                .ToListAsync();
            return new ResponseValue<PagedResult<ServiceDTO>>(
                new PagedResult<ServiceDTO>
                {
                    TotalItems = totalItems,
                    Page = 0,
                    PageSize = 0,
                    Items = services,
                },
                StatusReponse.Success,
                "Lấy danh sách dịch vụ thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<PagedResult<ServiceDTO>>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy danh sách dịch vụ: " + ex.Message
            );
        }
    }

    // ----------------------------------------
    //lấy danh sách hàng chờ của mình
    public async Task<List<DoctorQueueDto>> GetMyQueueTodayAsync(
        int doctorId,
        DateOnly? date = null
    )
    {
        var today = date ?? DateOnly.FromDateTime(DateTime.Now);

        // Lấy phòng làm việc của bác sĩ hôm nay
        var roomId = await _staffScheduleRepository
            .GetAll()
            .Where(ss => ss.StaffId == doctorId && ss.WorkDate == today)
            .Select(ss => ss.RoomId)
            .FirstOrDefaultAsync();

        if (roomId == null)
            return new List<DoctorQueueDto>();

        // ✅ CHỈ LẤY queue có appointment thuộc đúng bác sĩ
        return await _uow.GetDbContext()
            .Queues.Where(q => q.RoomId == roomId && q.QueueDate == today)
            .Join(
                _uow.GetDbContext().Appointments,
                q => q.AppointmentId,
                a => a.AppointmentId,
                (q, a) => new { q, a }
            )
            .Where(x => x.a.StaffId == doctorId) // ✅ Chỉ của bác sĩ hiện tại
            .Join(
                _uow.GetDbContext().Users,
                x => x.q.PatientId,
                u => u.UserId,
                (x, u) =>
                    new DoctorQueueDto
                    {
                        QueueId = x.q.QueueId,
                        QueueNumber = x.q.QueueNumber,
                        AppoinmentId = x.a.AppointmentId,
                        PatientName = u.FullName,
                        QueueTime = x.q.QueueTime,
                        Status = x.q.Status,
                    }
            )
            .OrderBy(q => q.QueueNumber)
            .ToListAsync();
    }
}
