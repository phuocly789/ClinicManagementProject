// using ClinicManagement_API.Models;
// using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IUserService : IServiceBase<User1>
{
    Task<User1?> GetByUsernameAsync(string username);
}

public class UserService : ServiceBase<User1>, IUserService
{
    public UserService(IUnitOfWork uow)
        : base(uow) { }

    //các crud cơ bản đã có trong ServiceBase
    //
    //nghiệp vụ đặc thù của User2 nếu có thì viết thêm ở đây
    public async Task<User1?> GetByUsernameAsync(string username)
    {
        return await _repository.SingleOrDefaultAsync(u => u.Username == username);
    }
}
