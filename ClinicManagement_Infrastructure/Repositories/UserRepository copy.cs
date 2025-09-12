using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IRoleRepository : IRepository<Role>
{
    // Add custom methods for Role here if needed
}

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(SupabaseContext context)
        : base(context) { }
}
