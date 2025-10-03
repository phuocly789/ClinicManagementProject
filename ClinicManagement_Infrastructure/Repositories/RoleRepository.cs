using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IRoleRepository : IRepository<Role>
{
    // Add custom methods for Role here if needed
    Task<bool> RoleExistsAsync(string roleName);
}

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(SupabaseContext context)
        : base(context) { }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _dbSet.AsNoTracking().AnyAsync(r => r.RoleName == roleName);
    }
}
