using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IServiceOrderRepository : IRepository<ServiceOrder>
{
    // Add custom methods for ServiceOrder here if needed
}

public class ServiceOrderRepository : Repository<ServiceOrder>, IServiceOrderRepository
{
    public ServiceOrderRepository(SupabaseContext context)
        : base(context) { }
}
