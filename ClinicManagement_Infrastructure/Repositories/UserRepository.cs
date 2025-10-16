using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;

public interface IUserRepository : IRepository<User>
{
    // Add custom methods for User here if needed
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(SupabaseContext context)
        : base(context) { }
}
