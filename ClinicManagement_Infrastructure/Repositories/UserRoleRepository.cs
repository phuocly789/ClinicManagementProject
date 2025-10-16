using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;

public interface IUserRoleRepository : IRepository<UserRole>
{
    // Add custom methods for UserRole here if needed
}

public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(SupabaseContext context)
        : base(context) { }
}
