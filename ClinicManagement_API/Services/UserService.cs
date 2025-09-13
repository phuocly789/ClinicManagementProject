// using ClinicManagement_API.Models;
// using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using dotnet03WebApi_EbayProject.Helper;

public interface IUserService : IServiceBase<User1>
{
    Task<User1?> GetByUsernameAsync(string username);
    Task<(bool Success, string Message)> Login(UserLoginDTO model);
    // Task<(bool Success, string Message)> RegisterBuyer(UserBuyerRegister registerBuyer);
    // Task<(bool Success, string Message)> RegisterSeller(UserSellerRegister registerSeller);
}

public class UserService : ServiceBase<User1>, IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _uow;
    JwtAuthService _jwtAuthService;

    public UserService(
        IUnitOfWork uow,
        JwtAuthService jwtAuthService,
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository
    )
        : base(uow)
    {
        _uow = uow;
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _jwtAuthService = jwtAuthService;
    }

    //các crud cơ bản đã có trong ServiceBase
    //
    //nghiệp vụ đặc thù của User2 nếu có thì viết thêm ở đây
    public async Task<User1?> GetByUsernameAsync(string username)
    {
        return await _repository.SingleOrDefaultAsync(u => u.Username == username);
    }

    public async Task<(bool Success, string Message)> Login(UserLoginDTO model)
    {
        if (string.IsNullOrWhiteSpace(model.EmailOrPhone))
            return (false, MessageLogin.UserNameOrEmailRequired);

        if (string.IsNullOrWhiteSpace(model.Password))
            return (false, MessageLogin.PasswordRequired);

        try
        {
            var user = await _userRepository.SingleOrDefaultAsync(u =>
                u.Username == model.EmailOrPhone || u.Email == model.EmailOrPhone
            );

            if (user == null)
                return (false, MessageLogin.UserNotFound);

            if (!PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
                return (false, MessageLogin.InvalidCredentials);

            string token = _jwtAuthService.GenerateToken(user);
            return (true, token);
        }
        catch
        {
            return (false, MessageLogin.LoginFailed);
        }
    }

    // public async Task<(bool Success, string Message)> Register(UserRegisterDTO registerBuyer)
    // {
    //     // Kiểm tra dữ liệu đầu vào
    //     if (string.IsNullOrWhiteSpace(registerBuyer.userName))
    //         return (false, "Username is required.");

    //     if (string.IsNullOrWhiteSpace(registerBuyer.email))
    //         return (false, "Email is required.");

    //     if (string.IsNullOrWhiteSpace(registerBuyer.password))
    //         return (false, "Password is required.");

    //     // Kiểm tra trùng username
    //     var existingUserByUsername = await _userRepository.SingleOrDefaultAsync(u =>
    //         u.Username == registerBuyer.userName
    //     );
    //     if (existingUserByUsername != null)
    //         return (false, "Username already exists.");

    //     // Kiểm tra trùng email
    //     var existingUserByEmail = await _userRepository.SingleOrDefaultAsync(u =>
    //         u.Email == registerBuyer.email
    //     );
    //     if (existingUserByEmail != null)
    //         return (false, "Email already exists.");

    //     try
    //     {
    //         var user = new User
    //         {
    //             Username = registerBuyer.userName,
    //             PasswordHash = PasswordHelper.HashPassword(registerBuyer.password),
    //             Email = registerBuyer.email,
    //             FullName = registerBuyer.fullName,
    //             UserRoles = new List<UserRole>
    //             {
    //                 new UserRole { RoleId = registerBuyer.getRoleId() },
    //             },
    //         };

    //         await _userRepository.AddAsync(user);
    //         await _unitOfWork.SaveChangesAsync();
    //         return (true, "Registration successful.");
    //     }
    //     catch (Exception ex)
    //     {
    //         return (false, $"Registration failed: {ex.Message}");
    //     }
    // }
}
