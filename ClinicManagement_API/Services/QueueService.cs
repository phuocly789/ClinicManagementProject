using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR;

public interface IQueueService
{
    Task<List<QueueDto>> GetQueuesAsync(int roomId, DateOnly? date = null);
    Task<ResponseValue<QueueDTO>> AddToQueueAsync(QueueCreateDTO request, int createdBy);
    Task<ResponseValue<QueueDTO>> QueueUpdateStatusAsync(int queueId, QueueStatusUpdateDTO request);
}

public class QueueService : IQueueService
{
    private readonly IQueueRepository _queueRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<QueueService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IHubContext<QueueHub> _hubContext;

    public QueueService(
        IQueueRepository queueRepository,
        IAppointmentRepository appointmentRepository,
        IRoomRepository roomRepository,
        ILogger<QueueService> logger,
        IUnitOfWork uow,
        IHubContext<QueueHub> hubContext
    )
    {
        _queueRepository = queueRepository;
        _appointmentRepository = appointmentRepository;
        _roomRepository = roomRepository;
        _logger = logger;
        _uow = uow;
        _hubContext = hubContext;
    }

    public async Task<List<QueueDto>> GetQueuesAsync(int roomId, DateOnly? date = null)
    {
        try
        {
            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Now);

            return await _queueRepository.GetQueuesAsync(roomId, selectedDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching queues for room {RoomId}", roomId);
            throw;
        }
    }

    public async Task<ResponseValue<QueueDTO>> AddToQueueAsync(QueueCreateDTO request, int createdBy)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                return new ResponseValue<QueueDTO>(
                    null,
                    StatusReponse.NotFound,
                    "Không tìm thấy lịch hẹn."
                );
            }

            if (appointment.RoomId == null)
            {
                return new ResponseValue<QueueDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Lịch hẹn chưa được gán phòng."
                );
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            var roomId = appointment.RoomId.Value;

            // Lấy số thứ tự cao nhất của phòng hôm nay
            var maxQueue = await _queueRepository.GetMaxQueueNumberAsync(roomId, today);
            var newQueueNumber = maxQueue + 1;

            using var transaction = await _uow.BeginTransactionAsync();

            var now = DateTime.Now;
            var queue = new Queue
            {
                QueueNumber = newQueueNumber,
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                RoomId = roomId, // ✅ Lấy từ Appointment, không lấy từ client
                QueueDate = today,
                QueueTime = new TimeOnly(now.Hour, now.Minute, now.Second),
                Status = "Waiting",
                CreatedBy = createdBy
            };

            await _queueRepository.AddAsync(queue);
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients.Group($"room_{queue.RoomId}").SendAsync("QueueUpdated");
            return new ResponseValue<QueueDTO>(
                new QueueDTO
                {
                    QueueId = queue.QueueId,
                    QueueNumber = queue.QueueNumber,
                    AppoinmentId = queue.AppointmentId,
                    PatientId = queue.PatientId,
                    RoomId = queue.RoomId,
                    QueueDate = queue.QueueDate,
                    QueueTime = queue.QueueTime,
                    Status = queue.Status,
                    CreatedBy = queue.CreatedBy
                },
                StatusReponse.Success,
                "Tạo hàng chờ thành công."
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while adding to queue: {@Request}", request);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding to queue: {@Request}", request);
            return new ResponseValue<QueueDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<QueueDTO>> QueueUpdateStatusAsync(
        int queueId,
        QueueStatusUpdateDTO request
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return new ResponseValue<QueueDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thông tin trạng thái không hợp lệ."
                );
            }

            var queue = await _queueRepository.GetByIdAsync(queueId);
            if (queue == null)
            {
                return new ResponseValue<QueueDTO>(
                    null,
                    StatusReponse.NotFound,
                    "Không tìm thấy hàng chờ."
                );
            }

            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                // ✅ Cập nhật trạng thái hàng chờ
                queue.Status = request.Status;
                await _queueRepository.Update(queue);

                // ✅ Nếu có Appointment đi kèm → cập nhật luôn
                if (queue.AppointmentId is int appointmentId)
                {
                    var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
                    if (appointment != null)
                    {
                        appointment.Status = request.Status;
                        await _appointmentRepository.Update(appointment);
                    }
                }

                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                // ✅ Gửi real-time update SignalR
                await _hubContext.Clients.Group($"room_{queue.RoomId}").SendAsync("QueueUpdated");

                return new ResponseValue<QueueDTO>(
                    null,
                    StatusReponse.Success,
                    "Cập nhật trạng thái thành công"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseValue<QueueDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi cập nhật trạng thái: " + ex.Message
                );
            }
        }
        catch (Exception ex)
        {
            return new ResponseValue<QueueDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi cập nhật trạng thái: " + ex.Message
            );
        }
    }
}
