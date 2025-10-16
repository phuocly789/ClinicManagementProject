using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;
public interface INotificationRepository : IRepository<Notification>
{
    // Add custom methods for Notification here if needed
}

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(SupabaseContext context)
        : base(context) { }
}
