using System.Transactions;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IScheduleService
{
    Task<ResponseValue<CreateScheduleRequestDTO>> CreateScheduleAsync(
        CreateScheduleRequestDTO request
    );
    Task<ResponseValue<UpdateScheduleRequestDTO>> UpdateScheduleAsync(
        int scheduleId,
        UpdateScheduleRequestDTO request
    );
    Task<ResponseValue<PagedResult<ScheduleForMedicalStaffResponse>>> GetAllSchedulesAsync(
     
    );
    Task<ResponseValue<bool>> DeleteScheduleAsync(int scheduleId);
}

public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _uow;
    private readonly IStaffScheduleRepository _staffScheduleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMedicalStaffRepository _medicalStaffRepository;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(
        IUnitOfWork uow,
        IStaffScheduleRepository staffScheduleRepository,
        IUserRepository userRepository,
        IMedicalStaffRepository medicalStaffRepository,
        ILogger<ScheduleService> logger
    )
    {
        _uow = uow;
        _staffScheduleRepository = staffScheduleRepository;
        _medicalStaffRepository = medicalStaffRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ResponseValue<CreateScheduleRequestDTO>> CreateScheduleAsync(
        CreateScheduleRequestDTO request
    )
    {
        try
        {
            //parse input
            if (
                !DateTime.TryParse(request.WorkDate, out var workDate)
                || !TimeOnly.TryParse(request.StartTime, out var startTime)
                || !TimeOnly.TryParse(request.EndTime, out var endTime)
            )
            {
                return new ResponseValue<CreateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Invalid date or time format."
                );
            }

            if (startTime >= endTime)
            {
                return new ResponseValue<CreateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Start time must be before end time."
                );
            }
            bool isNotPatient = await _medicalStaffRepository
                .GetAll()
                .AnyAsync(ms => ms.StaffId == request.StaffId && ms.StaffType != "Patient");
            bool hasOverlappingSchedule = await _staffScheduleRepository
                .GetAll()
                .AnyAsync(s =>
                    s.StaffId == request.StaffId
                    && s.WorkDate == DateOnly.FromDateTime(workDate)
                    && (
                        (s.StartTime < endTime && s.EndTime > startTime)
                        || (startTime < s.EndTime && endTime > s.StartTime)
                    )
                );

            //
            if (!isNotPatient)
            {
                return new ResponseValue<CreateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "StaffId Là Bệnh nhân, Không thể thêm lịch."
                );
            }
            if (hasOverlappingSchedule)
            {
                return new ResponseValue<CreateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "The new schedule overlaps with an existing schedule."
                );
            }
            //transaction
            using var transaction = await _uow.BeginTransactionAsync();

            var schedule = new StaffSchedule
            {
                StaffId = request.StaffId,
                WorkDate = DateOnly.FromDateTime(workDate),
                StartTime = startTime,
                EndTime = endTime,
                IsAvailable = request.IsAvailable,
            };
            //save
            await _staffScheduleRepository.AddAsync(schedule);
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();
            //return
            return new ResponseValue<CreateScheduleRequestDTO>(
                new CreateScheduleRequestDTO
                {
                    StaffId = schedule.StaffId,
                    WorkDate = schedule.WorkDate.ToString("yyyy-MM-dd"),
                    StartTime = schedule.StartTime.ToString("HH:mm:ss"),
                    EndTime = schedule.EndTime.ToString("HH:mm:ss"),
                    IsAvailable = schedule.IsAvailable,
                },
                StatusReponse.Success,
                "Schedule created successfully."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schedule");
            return new ResponseValue<CreateScheduleRequestDTO>(
                null,
                StatusReponse.Error,
                "An error occurred while creating the schedule: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<UpdateScheduleRequestDTO>> UpdateScheduleAsync(
        int scheduleId,
        UpdateScheduleRequestDTO request
    )
    {
        try
        {
            // Parse input
            if (
                !DateTime.TryParse(request.WorkDate, out var workDate)
                || !TimeOnly.TryParse(request.StartTime, out var startTime)
                || !TimeOnly.TryParse(request.EndTime, out var endTime)
            )
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Invalid date or time format."
                );
            }

            if (startTime >= endTime)
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Start time must be before end time."
                );
            }

            // Kiểm tra lịch tồn tại
            var schedule = await _staffScheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.NotFound,
                    "Schedule not found."
                );
            }

            // Kiểm tra lịch hiện tại có cùng ngày với request không
            if (schedule.WorkDate != DateOnly.FromDateTime(workDate))
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Can only update schedule for the same date."
                );
            }

            // Kiểm tra StaffId có phải bác sĩ
            bool isNotPatient = await _medicalStaffRepository
                .GetAll()
                .AnyAsync(ms => ms.StaffId == schedule.StaffId && ms.StaffType != "Patient");
            if (!isNotPatient)
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "StaffId does correspond to a valid Patient."
                );
            }

            // Kiểm tra trùng lặp lịch
            bool hasOverlappingSchedule = await _staffScheduleRepository
                .GetAll()
                .AnyAsync(s =>
                    s.StaffId == schedule.StaffId
                    && s.ScheduleId != scheduleId // Loại trừ lịch hiện tại
                    && s.WorkDate == DateOnly.FromDateTime(workDate)
                    && (
                        (s.StartTime < endTime && s.EndTime > startTime)
                        || (startTime < s.EndTime && endTime > s.StartTime)
                    )
                );

            if (hasOverlappingSchedule)
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "The new schedule overlaps with an existing schedule."
                );
            }

            // Cập nhật lịch
            schedule.WorkDate = DateOnly.FromDateTime(workDate);
            schedule.StartTime = startTime;
            schedule.EndTime = endTime;
            schedule.IsAvailable = request.IsAvailable;

            // Transaction
            using var transaction = await _uow.BeginTransactionAsync();
            await _staffScheduleRepository.Update(schedule);
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResponseValue<UpdateScheduleRequestDTO>(
                new UpdateScheduleRequestDTO
                {
                    WorkDate = schedule.WorkDate.ToString("yyyy-MM-dd"),
                    StartTime = schedule.StartTime.ToString("HH:mm:ss"),
                    EndTime = schedule.EndTime.ToString("HH:mm:ss"),
                    IsAvailable = schedule.IsAvailable,
                },
                StatusReponse.Success,
                "Schedule updated successfully."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating schedule {ScheduleId}", scheduleId);
            return new ResponseValue<UpdateScheduleRequestDTO>(
                null,
                StatusReponse.Error,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<
        ResponseValue<PagedResult<ScheduleForMedicalStaffResponse>>
    > GetAllSchedulesAsync()
    {
        try
        {
           
            var query =
                from schedule in _staffScheduleRepository.GetAll()
                join user in _userRepository.GetAll()
                    on schedule.StaffId equals user.UserId
                    into staffUsers
                from user in staffUsers.DefaultIfEmpty()
                select new { schedule, user };

            var totalItems = await query.CountAsync();

            var schedules = await query
                .OrderBy(q => q.schedule.ScheduleId)
                .Select(q => new ScheduleForMedicalStaffResponse
                {
                    StaffId = q.schedule.StaffId,
                    StaffName = q.user != null ? q.user.FullName : "(Không xác định)",
                    Role = q.user.UserRoles.Select(r => r.Role.RoleName).FirstOrDefault(),

                    WorkDate = q.schedule.WorkDate.ToString("yyyy-MM-dd"),
                    StartTime = q.schedule.StartTime.ToString("HH:mm:ss"),
                    EndTime = q.schedule.EndTime.ToString("HH:mm:ss"),
                    IsAvailable = q.schedule.IsAvailable,
                })
                .ToListAsync();

            return new ResponseValue<PagedResult<ScheduleForMedicalStaffResponse>>(
                new PagedResult<ScheduleForMedicalStaffResponse>
                {
                    TotalItems = totalItems,
                    Page = 0,
                    PageSize = 0,
                    Items = schedules,
                },
                StatusReponse.Success,
                "Lấy danh sách lịch thành công."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllSchedulesAsync");
            return new ResponseValue<PagedResult<ScheduleForMedicalStaffResponse>>(
                null,
                StatusReponse.Error,
                "An error occurred while processing your request." + ex.Message
            );
        }
    }

    public async Task<ResponseValue<bool>> DeleteScheduleAsync(int scheduleId)
    {
        try
        {
            var schedule = await _staffScheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                return new ResponseValue<bool>(
                    false,
                    StatusReponse.NotFound,
                    "Schedule not found."
                );
            }
            using var transaction = await _uow.BeginTransactionAsync();
            await _staffScheduleRepository.DeleteAsync(schedule);
            await transaction.CommitAsync();
            await _uow.SaveChangesAsync();

            return new ResponseValue<bool>(
                true,
                StatusReponse.Success,
                "Schedule deleted successfully."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteScheduleAsync");
            return new ResponseValue<bool>(false, StatusReponse.Error, ex.Message);
        }
    }
}
