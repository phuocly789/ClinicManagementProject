using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;

public interface IUserOtpRepository : IRepository<UserOtp>
{
    // Add custom methods for UserOtp here if needed
}

public class UserOtpRepository : Repository<UserOtp>, IUserOtpRepository
{
    public UserOtpRepository(SupabaseContext context)
        : base(context) { }
}
