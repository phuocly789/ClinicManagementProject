using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IUserRepository : IRepository<User1>
{
    // Add custom methods for User here if needed
}

public class UserRepository : Repository<User1>, IUserRepository
{
    public UserRepository(SupabaseContext context)
        : base(context) { }
}
