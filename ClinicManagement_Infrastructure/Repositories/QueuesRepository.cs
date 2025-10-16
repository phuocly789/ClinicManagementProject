using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;
public interface IQueueRepository : IRepository<Queue>
{
    // Add custom methods for Queue here if needed
}

public class QueueRepository : Repository<Queue>, IQueueRepository
{
    public QueueRepository(SupabaseContext context)
        : base(context) { }
}
