using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(int userId);
    Task<bool> VerifyOtpAsync(int userId, string otp);
}

public class OtpService : IOtpService
{
    private readonly IUserOtpRepository _userOtpRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _cache;
    private readonly IUserRepository _userRepository;

    public OtpService(
        IUnitOfWork uow,
        IUserOtpRepository userOtpRepository,
        IMemoryCache cache,
        IUserRepository userRepository
    )
    {
        _uow = uow;
        _userOtpRepository = userOtpRepository;
        _cache = cache;
        _userRepository = userRepository;
    }

    public async Task<string> GenerateOtpAsync(int userId)
    {
        //Chống spam
        var cacheKeyCooldown = $"otp_cooldown_{userId}";
        var cacheKeySendCount = $"otp_send_code_{userId}_{DateTime.UtcNow:yyyyMMdd}";

        //không cho gửi lại email nếu chưa đủ thời gian
        if (_cache.TryGetValue(cacheKeyCooldown, out _))
        {
            return "COOLDOWN";
        }

        //giới hạn số lần gửi lại trong ngày
        var sendCount = _cache.Get<int?>(cacheKeySendCount) ?? 0;
        if (sendCount >= 10)
        {
            return "LIMIT_EXCEEDED";
        }

        var otp = new Random().Next(100000, 999999).ToString(); // 6 digits

        var entry = new UserOtp
        {
            UserId = userId,
            Otpcode = otp,
            ExpiredAt = DateTime.UtcNow.AddMinutes(3),
            IsUsed = false,
        };

        await _userOtpRepository.AddAsync(entry);
        await _uow.SaveChangesAsync();

        // ✅ Set cooldown 60 giây
        _cache.Set(cacheKeyCooldown, true, TimeSpan.FromSeconds(120));

        // ✅ Tăng số lần gửi trong ngày (expire 24h)
        _cache.Set(cacheKeySendCount, sendCount + 1, TimeSpan.FromHours(24));

        return otp;
    }

    public async Task<bool> VerifyOtpAsync(int userId, string otp)
    {
        var cacheKeyAttempt = $"otp_attempt_{userId}";

        var attempts = _cache.Get<int?>(cacheKeyAttempt) ?? 0;
        if (attempts >= 5)
            throw new Exception("Bạn đã nhập sai quá nhiều lần. Vui lòng yêu cầu lại mã mới.");

        // 🔍 Lấy OTP gần nhất
        var record = await _userOtpRepository
            .GetAll()
            .Where(o => o.UserId == userId && o.Otpcode == otp && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (record == null || record.ExpiredAt < DateTime.UtcNow)
        {
            // tăng số lần nhập sai
            _cache.Set(cacheKeyAttempt, attempts + 1, TimeSpan.FromMinutes(5));
            return false;
        }

        // ✅ Đánh dấu đã dùng OTP
        record.IsUsed = true;
        await _userOtpRepository.Update(record);

        // ✅ Kích hoạt tài khoản *sau khi OTP đúng*
        var user = await _userRepository.SingleOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            throw new Exception("Người dùng không tồn tại");

        user.IsActive = true;
        await _userRepository.Update(user);

        await _uow.SaveChangesAsync(); // ✅ SAVE

        // xóa cooldown & đếm sai
        _cache.Remove($"otp_cooldown_{userId}");
        _cache.Remove(cacheKeyAttempt);

        return true;
    }
}
