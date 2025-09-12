using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IStaffScheduleRepository : IRepository<StaffSchedule>
{
    // Add custom methods for StaffSchedule here if needed
}

public class StaffScheduleRepository : Repository<StaffSchedule>, IStaffScheduleRepository
{
    public StaffScheduleRepository(SupabaseContext context)
        : base(context) { }
}
