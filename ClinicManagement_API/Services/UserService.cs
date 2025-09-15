// using ClinicManagement_API.Models;
// using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using dotnet03WebApi_EbayProject.Helper;
using Microsoft.EntityFrameworkCore;

public interface IUserService : IServiceBase<User1>
{
    Task<User1?> GetByUsernameAsync(string username);
    Task<ResponseValue<LoginResponseDTO>> Login(UserLoginDTO model);
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

    public async Task<ResponseValue<LoginResponseDTO>> Login(UserLoginDTO model)
    {
        // Lấy repository cho User1
        var userRepo = _uow.Repository<User1>();

        // Ép repository thành IQueryable để dùng Include
        var queryable = userRepo.Query();

        var user = await queryable
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u =>
                u.Username == model.EmailOrPhone || u.Email == model.EmailOrPhone
            );
        if (user == null)
            return new ResponseValue<LoginResponseDTO>(
                null,
                StatusReponse.NotFound,
                "User not found"
            );

        if (!PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
            return new ResponseValue<LoginResponseDTO>(
                null,
                StatusReponse.BadRequest,
                "Invalid password"
            );

        // Kiểm tra role
        var hasRole = user.UserRoles.Any(ur => ur.Role.RoleName == model.Role);
        if (!hasRole)
            return new ResponseValue<LoginResponseDTO>(
                null,
                StatusReponse.BadRequest,
                "User does not have the required role"
            );

        // Tạo token với role được chọn
        var token = _jwtAuthService.GenerateToken(user, new List<string> { model.Role });

        return new ResponseValue<LoginResponseDTO>(
            new LoginResponseDTO
            {
                Token = token,
                Roles = new List<string> { model.Role },
            },
            StatusReponse.Success,
            "Login successful"
        );
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
