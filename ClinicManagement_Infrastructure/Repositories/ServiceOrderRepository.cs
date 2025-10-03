using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IServiceOrderRepository : IRepository<ServiceOrder>
{
    // Add custom methods for ServiceOrder here if needed
    Task<bool> IsValidAppointmentAsync(int appointmentId, int staffId);
    Task<List<ServiceOrder>> GetCompletedServiceOrdersAsync(int appointmentId);
    Task<List<Service>> GetServicesByIdsAsync(List<int?> serviceIds);
    Task<List<ServiceOrder>> GetAssignedServiceOrdersAsync(int staffId, DateTime? date);
    Task<List<User1>> GetUsersByIdsAsync(List<int?> userIds);
}

public class ServiceOrderRepository : Repository<ServiceOrder>, IServiceOrderRepository
{
    public ServiceOrderRepository(SupabaseContext context)
        : base(context) { }

    public async Task<bool> IsValidAppointmentAsync(int appointmentId, int staffId)
    {
        return await _context.Appointments.AnyAsync(a =>
            a.AppointmentId == appointmentId && a.StaffId == staffId
        );
    }

    public async Task<List<ServiceOrder>> GetCompletedServiceOrdersAsync(int appointmentId)
    {
        return await _context
            .ServiceOrders.Where(so =>
                so.AppointmentId == appointmentId && so.Status == "Completed"
            )
            .ToListAsync();
    }

    public async Task<List<Service>> GetServicesByIdsAsync(List<int?> serviceIds)
    {
        return await _context.Services.Where(s => serviceIds.Contains(s.ServiceId)).ToListAsync();
    }

    public async Task<List<ServiceOrder>> GetAssignedServiceOrdersAsync(int staffId, DateTime? date)
    {
        var query = _context.ServiceOrders.Where(so => so.AssignedStaffId == staffId);

        if (date.HasValue)
        {
            var startDate = date.Value.Date;
            var endDate = startDate.AddDays(1);
            query = query.Where(so => so.OrderDate >= startDate && so.OrderDate < endDate);
        }

        return await query.ToListAsync();
    }

    public async Task<List<User1>> GetUsersByIdsAsync(List<int?> userIds)
    {
        return await _context.Users1.Where(u => userIds.Contains(u.UserId)).ToListAsync();
    }
}
