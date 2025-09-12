using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IAppointmentRepository : IRepository<Appointment>
{
    // Add custom methods for Appointment here if needed
}

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(SupabaseContext context)
        : base(context) { }
}
