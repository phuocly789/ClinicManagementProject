using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IQueueService
{
    Task<List<QueueDto>> GetQueuesAsync(int roomId, DateOnly? date = null);
    Task<ResponseValue<QueueDTO>> AddToQueueAsync(QueueCreateDTO request);
    Task<ResponseValue<QueueDTO>> QueueUpdateStatusAsync(int queueId, QueueStatusUpdateDTO request);
}
public class QueueService : IQueueService
{
    private readonly IQueueRepository _queueRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<QueueService> _logger;
    private readonly IUnitOfWork _uow;

    public QueueService(
        IQueueRepository queueRepository,
        IAppointmentRepository appointmentRepository,
        IRoomRepository roomRepository,
        ILogger<QueueService> logger,
        IUnitOfWork uow
    )
    {
        _queueRepository = queueRepository;
        _appointmentRepository = appointmentRepository;
        _roomRepository = roomRepository;
        _logger = logger;
        _uow = uow;
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

    public async Task<ResponseValue<QueueDTO>> AddToQueueAsync(QueueCreateDTO request)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                throw new ArgumentException("Không tìm thấy lịch hẹn");
            }

            var room = await _roomRepository.GetByIdAsync(request.RoomId);
            if (room == null)
            {
                throw new ArgumentException("Không tìm thấy phòng");
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            var maxQueue = await _queueRepository.GetMaxQueueNumberAsync(request.RoomId, today);
            var newQueueNumber = maxQueue + 1;
            var now = DateTime.Now;

            using var transaction = await _uow.BeginTransactionAsync();

            var queue = new Queue
            {
                QueueNumber = newQueueNumber,
                AppointmentId = request.AppointmentId,
                PatientId = appointment.PatientId,
                RoomId = request.RoomId,
                QueueDate = today,
                QueueTime = new TimeOnly(now.Hour, now.Minute, now.Second),
                Status = "Waiting",
            };

            await _queueRepository.AddAsync(queue);
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResponseValue<QueueDTO>(
                new QueueDTO
                {
                    QueueId = queue.QueueId,
                    QueueNumber = queue.QueueNumber,
                    PatientId = queue.PatientId,
                    RoomId = queue.RoomId,
                    QueueDate = queue.QueueDate,
                    QueueTime = queue.QueueTime,
                    Status = queue.Status
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

    public async Task<ResponseValue<QueueDTO>> QueueUpdateStatusAsync(int queueId, QueueStatusUpdateDTO request)
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
                queue.Status = request.Status;

                await _queueRepository.Update(queue);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

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