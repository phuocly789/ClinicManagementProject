using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;

public interface IRoomRepository : IRepository<Room>
{
    // Add custom methods for Room here if needed
}

public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(SupabaseContext context)
        : base(context) { }
}
