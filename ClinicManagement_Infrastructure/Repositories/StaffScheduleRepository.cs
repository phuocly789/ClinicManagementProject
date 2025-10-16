using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;

public interface IStaffScheduleRepository : IRepository<StaffSchedule>
{
    // Add custom methods for StaffSchedule here if needed
}

public class StaffScheduleRepository : Repository<StaffSchedule>, IStaffScheduleRepository
{
    public StaffScheduleRepository(SupabaseContext context)
        : base(context) { }
}
