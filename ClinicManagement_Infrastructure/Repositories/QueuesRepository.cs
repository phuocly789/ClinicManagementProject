using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IQueueRepository : IRepository<Queue>
{
    // Add custom methods for Queue here if needed
}

public class QueueRepository : Repository<Queue>, IQueueRepository
{
    public QueueRepository(SupabaseContext context)
        : base(context) { }
}
