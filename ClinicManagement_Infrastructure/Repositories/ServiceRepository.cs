using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IServiceRepository : IRepository<Service>
{
    // Add custom methods for Service here if needed
}

public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(SupabaseContext context)
        : base(context) { }
}
