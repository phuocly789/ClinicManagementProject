// using ClinicManagement_API.Models;
// using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

using System.Text.RegularExpressions;
using ClinicManagement_Infrastructure.Data.Models;
using dotnet03WebApi_EbayProject.Helper;
using Microsoft.EntityFrameworkCore;

public interface IUserService : IServiceBase<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<ResponseValue<UserDTO>> ChangePasswordAsync(
        string username,
        string currentPassword,
        string newPassword
    );
    Task<ResponseValue<UserUpdateDTO>> UpdateUserAsync(UserUpdateDTO request, string username);

    Task<ResponseValue<LoginResponseDTO>> Login(UserLoginDTO model);
    // Task<(bool Success, string Message)> RegisterBuyer(UserBuyerRegister registerBuyer);
    // Task<(bool Success, string Message)> RegisterSeller(UserSellerRegister registerSeller);
}

public class UserService : ServiceBase<User>, IUserService
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
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _repository.SingleOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _repository.SingleOrDefaultAsync(u => u.Email == email);
    }

    public async Task<ResponseValue<LoginResponseDTO>> Login(UserLoginDTO model)
    {
        // Lấy repository cho User1
        var userRepo = _uow.Repository<User>();

        // Ép repository thành IQueryable để dùng Include
        var queryable = userRepo.Query();

        var user = await queryable
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u =>
                u.Phone == model.EmailOrPhone || u.Email == model.EmailOrPhone
            );
        if (user == null)
            return new ResponseValue<LoginResponseDTO>(
                null,
                StatusReponse.NotFound,
                "Không tìm thấy người dùng trong hệ thống"
            );

        if (!PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
            return new ResponseValue<LoginResponseDTO>(
                null,
                StatusReponse.BadRequest,
                "Mật khẩu không đúng"
            );

        // Kiểm tra role
        var hasRole = user.UserRoles.Any(ur => ur.Role.RoleName == model.Role);
        if (!hasRole)
            return new ResponseValue<LoginResponseDTO>(
                null,
                StatusReponse.BadRequest,
                "Vui lòng chọn vai trò phù hợp"
            );
        if (!(user.IsActive ?? false))
        {
            return new ResponseValue<LoginResponseDTO>(
                null,
                StatusReponse.BadRequest,
                "Tài khoản chưa được kích hoạt"
            );
        }
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

    public async Task<ResponseValue<UserDTO>> ChangePasswordAsync(
        string username,
        string currentPassword,
        string newPassword
    )
    {
        var user = await GetByUsernameAsync(username);

        if (user == null)
        {
            return new ResponseValue<UserDTO>(
                null,
                StatusReponse.NotFound,
                "Không tìm thấy người dùng"
            );
        }
        if (!PasswordHelper.VerifyPassword(currentPassword, user.PasswordHash))
        {
            return new ResponseValue<UserDTO>(
                null,
                StatusReponse.BadRequest,
                "Mật khẩu hiện tại không đúng"
            );
        }
        if (currentPassword == newPassword)
        {
            return new ResponseValue<UserDTO>(
                null,
                StatusReponse.BadRequest,
                "Mật khẩu mới không được trùng với mật khẩu cũ"
            );
        }
        //mã khóa mật khẩu mới
        using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            user.MustChangePassword = false;
            await _userRepository.Update(user);

            await transaction.CommitAsync();
            await _uow.SaveChangesAsync();

            return new ResponseValue<UserDTO>(
                null,
                StatusReponse.Success,
                "Đổi mật khẩu thành công"
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResponseValue<UserDTO>(null, StatusReponse.Error, ex.Message);
        }
    }

    public async Task<ResponseValue<UserUpdateDTO>> UpdateUserAsync(
        UserUpdateDTO request,
        string username
    )
    {
        var user = await GetByUsernameAsync(username);

        if (user == null)
        {
            return new ResponseValue<UserUpdateDTO>(
                null,
                StatusReponse.NotFound,
                "Không tìm thấy người dùng."
            );
        }

        // === VALIDATE INPUT ===
        if (string.IsNullOrWhiteSpace(request.FullName))
            return new ResponseValue<UserUpdateDTO>(
                null,
                StatusReponse.BadRequest,
                "Họ tên không được để trống."
            );

        if (string.IsNullOrWhiteSpace(request.Phone))
            return new ResponseValue<UserUpdateDTO>(
                null,
                StatusReponse.BadRequest,
                "Số điện thoại không được để trống."
            );

        if (!Regex.IsMatch(request.Phone, @"^0\d{9,10}$"))
            return new ResponseValue<UserUpdateDTO>(
                null,
                StatusReponse.BadRequest,
                "Số điện thoại không hợp lệ."
            );

        if (request.DateOfBirth > DateTime.UtcNow.Date)
            return new ResponseValue<UserUpdateDTO>(
                null,
                StatusReponse.BadRequest,
                "Ngày sinh không hợp lệ."
            );

        if (string.IsNullOrWhiteSpace(request.Gender))
            return new ResponseValue<UserUpdateDTO>(
                null,
                StatusReponse.BadRequest,
                "Giới tính không được để trống."
            );

        // === Check phone có bị user khác dùng không ===
        var phoneOwner = await _userRepository.SingleOrDefaultAsync(x => x.Phone == request.Phone);
        if (phoneOwner != null && phoneOwner.UserId != user.UserId)
        {
            return new ResponseValue<UserUpdateDTO>(
                null,
                StatusReponse.BadRequest,
                "Số điện thoại này đã được sử dụng bởi tài khoản khác."
            );
        }

        await using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.Gender = request.Gender;
            user.Address = request.Address;
            user.DateOfBirth = DateOnly.FromDateTime(request.DateOfBirth);

            await _userRepository.Update(user);

            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResponseValue<UserUpdateDTO>(
                new UserUpdateDTO
                {
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Gender = user.Gender,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth.HasValue
                        ? user.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                        : default,
                },
                StatusReponse.Success,
                "Cập nhật thông tin thành công."
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ResponseValue<UserUpdateDTO>(
                null,
                StatusReponse.Error,
                "Lỗi hệ thống: " + ex.Message
            );
        }
    }
}
