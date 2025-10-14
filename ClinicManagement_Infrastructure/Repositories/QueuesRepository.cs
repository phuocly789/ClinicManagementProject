using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IQueueRepository : IRepository<Queue>
{
    Task<List<QueueDto>> GetQueuesAsync(int roomId, DateOnly date);
    Task<int> GetMaxQueueNumberAsync(int roomId, DateOnly date);
}

public class QueueRepository : Repository<Queue>, IQueueRepository
{
    public QueueRepository(SupabaseContext context)
        : base(context) { }

    public async Task<List<QueueDto>> GetQueuesAsync(int roomId, DateOnly date)
    {
        return await _context.Queues
            .Where(q => q.RoomId == roomId && q.QueueDate == date)
            .Join(
                _context.Users1,
                q => q.PatientId,
                u => u.UserId,
                (q, u) => new { Queue = q, User = u }
            )
            .Join(
                _context.Rooms,
                qu => qu.Queue.RoomId,
                r => r.RoomId,
                (qu, r) => new
                {
                    qu.Queue,
                    qu.User,
                    Room = r
                }
            )
            .Select(qur => new QueueDto
            {
                QueueId = qur.Queue.QueueId,
                PatientId = qur.Queue.PatientId,
                PatientName = qur.User.FullName,
                RoomId = qur.Queue.RoomId,
                RoomName = qur.Room.RoomName,
                QueueDate = qur.Queue.QueueDate,
                QueueTime = qur.Queue.QueueTime,
                Status = qur.Queue.Status
            })
            .OrderBy(q => q.QueueTime)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetMaxQueueNumberAsync(int roomId, DateOnly date)
    {
        return await _context.Queues
            .Where(q => q.RoomId == roomId && q.QueueDate == date)
            .MaxAsync(q => (int?)q.QueueNumber) ?? 0;
    }
}

public class QueueDto
{
    public int QueueId { get; set; }
    public int QueueNumber { get; set; }
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public int? RoomId { get; set; }
    public string? RoomName { get; set; }
    public DateOnly QueueDate { get; set; }
    public TimeOnly QueueTime { get; set; }
    public string? Status { get; set; }
}

