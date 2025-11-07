using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IStaffScheduleRepository : IRepository<StaffSchedule>
{
    Task<bool> HasScheduleAsync(int staffId, DateOnly date);
}

public class StaffScheduleRepository : Repository<StaffSchedule>, IStaffScheduleRepository
{
    public StaffScheduleRepository(SupabaseContext context)
        : base(context) { }

    public async Task<bool> HasScheduleAsync(int staffId, DateOnly date)
    {
        return await _context.StaffSchedules
            .AnyAsync(s => s.StaffId == staffId && s.WorkDate == date);
    }
}
