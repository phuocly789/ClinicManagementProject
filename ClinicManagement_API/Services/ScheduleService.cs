using System.Transactions;
using ClinicManagement_Infrastructure.Data.Models;
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
    Task<ResponseValue<PagedResult<ScheduleForMedicalStaffResponse>>> GetAllSchedulesAsync();
    Task<ResponseValue<bool>> DeleteScheduleAsync(int scheduleId);
}

public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _uow;
    private readonly IStaffScheduleRepository _staffScheduleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMedicalStaffRepository _medicalStaffRepository;
    private readonly ILogger<ScheduleService> _logger;
    private readonly IRoomRepository _roomRepository;

    public ScheduleService(
        IUnitOfWork uow,
        IStaffScheduleRepository staffScheduleRepository,
        IUserRepository userRepository,
        IMedicalStaffRepository medicalStaffRepository,
        ILogger<ScheduleService> logger,
        IRoomRepository roomRepository
    )
    {
        _uow = uow;
        _staffScheduleRepository = staffScheduleRepository;
        _medicalStaffRepository = medicalStaffRepository;
        _userRepository = userRepository;
        _roomRepository = roomRepository;
        _logger = logger;
    }

    public async Task<ResponseValue<CreateScheduleRequestDTO>> CreateScheduleAsync(
        CreateScheduleRequestDTO request
    )
    {
        try
        {
            //kiểm tra phòng khám
            var room = await _roomRepository.GetByIdAsync(request.RoomId);
            if (room == null)
            {
                return new ResponseValue<CreateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Phòng không tồn tại."
                );
            }
            // //kiểm tra bác sĩ có lịch trong cùng phòng vào thời gian đó không
            // bool hasRoomConflict = await _staffScheduleRepository
            //     .GetAll()
            //     .AnyAsync(s =>
            //         s.StaffId == request.StaffId
            //         && s.WorkDate == DateOnly.Parse(request.WorkDate)
            //         && s.RoomId == request.RoomId
            //         && (
            //             (
            //                 s.StartTime < TimeOnly.Parse(request.EndTime)
            //                 && s.EndTime > TimeOnly.Parse(request.StartTime)
            //             )
            //             || (
            //                 TimeOnly.Parse(request.StartTime) < s.EndTime
            //                 && TimeOnly.Parse(request.EndTime) > s.StartTime
            //             )
            //         )
            //     );
            // if (hasRoomConflict)
            // {
            //     return new ResponseValue<CreateScheduleRequestDTO>(
            //         null,
            //         StatusReponse.BadRequest,
            //         "Bác sĩ đã có lịch trong cùng phòng"
            //     );
            // }
            var currentDate = DateTime.Now;
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
                    "Thời gian hoặc định dạng ngày không hợp lệ."
                );
            }
            if (
                workDate < currentDate
                || workDate.Date == currentDate.Date
                    && endTime <= TimeOnly.FromDateTime(currentDate)
            )
            {
                return new ResponseValue<CreateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Không thể tạo lịch trong quá khứ."
                );
            }
            if (startTime >= endTime)
            {
                return new ResponseValue<CreateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thời gian bắt đầu phải trước thời gian kết thúc."
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
                    && s.RoomId == request.RoomId
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
                    "Lịch bị trùng với lịch hiện có."
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
                RoomId = request.RoomId,
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
                "Tạo lịch thành công."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schedule");
            return new ResponseValue<CreateScheduleRequestDTO>(
                null,
                StatusReponse.Error,
                "Lỗi khi tạo lịch: " + ex.Message
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
            //kiểm tra phòng khám
            var room = await _roomRepository.GetByIdAsync(request.RoomId);
            if (room == null)
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Phòng không tồn tại."
                );
            }

            var currentDate = DateTime.Now;
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
                    "Lịch hoặc định dạng ngày không hợp lệ."
                );
            }
            if (
                workDate < currentDate
                || workDate.Date == currentDate.Date
                    && endTime <= TimeOnly.FromDateTime(currentDate)
            )
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Không thể tạo lịch trong quá khứ."
                );
            }
            if (startTime >= endTime)
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thời gian bắt đầu phải trước thời gian kết thúc."
                );
            }

            // Kiểm tra lịch tồn tại
            var schedule = await _staffScheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.NotFound,
                    "Không tìm thấy lịch."
                );
            }

            // Kiểm tra lịch hiện tại có cùng ngày với request không
            if (schedule.WorkDate != DateOnly.FromDateTime(workDate))
            {
                return new ResponseValue<UpdateScheduleRequestDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Chỉ có thể cập nhật lịch trong cùng một ngày làm việc."
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
                    "StaffId Là Bệnh nhân, Không thể cập nhật lịch."
                );
            }

            // Kiểm tra trùng lặp lịch
            bool hasOverlappingSchedule = await _staffScheduleRepository
                .GetAll()
                .AnyAsync(s =>
                    s.StaffId == schedule.StaffId
                    && s.ScheduleId != scheduleId // Loại trừ lịch hiện tại
                    && s.WorkDate == DateOnly.FromDateTime(workDate)
                    && s.RoomId == request.RoomId
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
                    "Lịch bị trùng với lịch hiện có."
                );
            }

            // Cập nhật lịch
            schedule.WorkDate = DateOnly.FromDateTime(workDate);
            schedule.StartTime = startTime;
            schedule.EndTime = endTime;
            schedule.RoomId = request.RoomId;
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
                "Cập nhật lịch thành công."
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
                join room in _roomRepository.GetAll()
                    on schedule.RoomId equals room.RoomId
                    into scheduleRooms
                from room in scheduleRooms.DefaultIfEmpty()
                select new
                {
                    schedule,
                    user,
                    room,
                };

            var totalItems = await query.CountAsync();

            var schedules = await query
                .OrderBy(q => q.schedule.ScheduleId)
                .Select(q => new ScheduleForMedicalStaffResponse
                {
                    ScheduleId = q.schedule.ScheduleId,
                    StaffId = q.schedule.StaffId,

                    StaffName = q.user != null ? q.user.FullName : "(Không xác định)",

                    Role =
                        q.user != null && q.user.UserRoles.Any()
                            ? q.user.UserRoles.Select(r => r.Role.RoleName).FirstOrDefault()
                            : null,

                    RoomId = q.schedule.RoomId,

                    RoomName = q.room != null ? q.room.RoomName : "(Không có phòng)",

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
                    "Không tìm thấy lịch."
                );
            }
            using var transaction = await _uow.BeginTransactionAsync();
            await _staffScheduleRepository.DeleteAsync(schedule);
            await transaction.CommitAsync();
            await _uow.SaveChangesAsync();

            return new ResponseValue<bool>(true, StatusReponse.Success, "Xóa lịch thành công.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteScheduleAsync");
            return new ResponseValue<bool>(false, StatusReponse.Error, ex.Message);
        }
    }
}
