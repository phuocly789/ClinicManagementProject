using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement_API.Models;
using ClinicManagement_Infrastructure.Data.Models;
using dotnet03WebApi_EbayProject.Helper;
using Microsoft.AspNetCore.Mvc;

//using ClinicManagement_API.Models;

namespace ClinicManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IUserRepository _userRepository;
        private readonly IPatinetService _patientService;
        private readonly IUnitOfWork _uow;

        public AuthController(
            IUserRepository userRepository,
            IUserService userService,
            JwtAuthService jwtAuthService,
            IPatinetService patientService,
            IOtpService otpService,
            IUnitOfWork uow,
            IEmailService emailService
        )
        {
            _userRepository = userRepository;
            _userService = userService;
            _jwtAuthService = jwtAuthService;
            _patientService = patientService;
            _otpService = otpService;
            _emailService = emailService;
            _uow = uow;
        }

        [HttpPost("UserLogin")]
        public async Task<ActionResult> UserLogin([FromBody] UserLoginDTO loginDto)
        {
            // Kiểm tra dữ liệu đầu vào
            if (loginDto == null)
                return BadRequest(new { Message = "Invalid login data." });

            // Gọi service để xử lý logic đăng nhập
            var result = await _userService.Login(loginDto);

            // Xử lý các trường hợp thất bại
            if (result.Status != StatusReponse.Success)
            {
                // Trả về BadRequest (400) cho các lỗi từ người dùng (ví dụ: sai mật khẩu, sai vai trò)
                if (result.Status == StatusReponse.BadRequest)
                    return BadRequest(new { Message = result.Message });

                // Trả về NotFound (404) nếu không tìm thấy người dùng
                if (result.Status == StatusReponse.NotFound)
                    return NotFound(new { Message = result.Message });

                // Trả về Internal Server Error (500) cho các lỗi khác
                return StatusCode(500, new { Message = result.Message });
            }

            // Trường hợp đăng nhập thành công
            return Ok(
                new LoginResponseDTO { Token = result.Content.Token, Roles = result.Content.Roles }
            );
        }

        // Endpoint for patient self-registration

        [HttpPost("PatientRegister")]
        // Đổi tên DTO cho khớp với code service tôi đã gửi
        public async Task<ActionResult<ResponseValue<PatientRegisterDto>>> PatientRegister(
            [FromBody] PatientRegisterDto registerDto
        )
        {
            if (!ModelState.IsValid)
            {
                // Trả về lỗi validation chi tiết
                return BadRequest(ModelState);
            }

            // Gọi đúng service và phương thức
            var result = await _patientService.RegisterPatientAsync(registerDto);

            // Kiểm tra kết quả và trả về response
            if (result.Status == StatusReponse.Success)
            {
                return Created("", result);
            }

            if (result.Status == StatusReponse.BadRequest)
            {
                return BadRequest(result);
            }

            // Mặc định các lỗi khác là lỗi server
            return StatusCode(500, result);
        }

        //OTP
        [HttpPost("SendOTP")]
        public async Task<IActionResult> SendOTP([FromBody] SendOtpRequest request)
        {
            var user = await _userRepository.SingleOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return NotFound("Email chưa được đăng ký trong hệ thống.");

            var otp = await _otpService.GenerateOtpAsync(user.UserId);
            if (otp == null)
            {
                return BadRequest("Vui lòng đợi trước khi yêu cầu lại mã OTP.");
            }
            if (otp == "COOLDOWN")
                return BadRequest("Vui lòng đợi 2 phút trước khi gửi lại mã!");

            if (otp == "LIMIT_EXCEEDED")
                return BadRequest("Bạn đã gửi quá 10 lần hôm nay. Thử lại ngày mai nhé!");

            var emailBody =
                $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Mã OTP VITACARE</title>
    <link href='https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap' rel='stylesheet'>
</head>
<body style='margin:0; padding:0; background:linear-gradient(135deg, #f5f7fa 0%, #e4edf5 100%); font-family:Inter,Arial,sans-serif'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background:linear-gradient(135deg, #f5f7fa 0%, #e4edf5 100%); padding:40px 0'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' style='max-width:520px; background:#ffffff; border-radius:20px; overflow:hidden; box-shadow:0 15px 40px rgba(74, 108, 247, 0.15); border:1px solid #e8f0fe'>
                    <!-- Header với gradient mới -->
                    <tr>
                        <td style='background:linear-gradient(135deg, #4a6cf7 0%, #7b4af7 100%); padding:45px 30px; text-align:center; position:relative'>
                            <div style='position:absolute; top:0; left:0; right:0; bottom:0; background-color:rgba(255,255,255,0.1);'></div>
                            <div style='position:relative; z-index:1'>
                                <h1 style='color:white; margin:0; font-size:32px; font-weight:700; letter-spacing:-0.5px'>
                                    VITACARE
                                </h1>
                                <p style='color:rgba(255,255,255,0.95); margin:12px 0 0; font-size:16px; font-weight:400'>
                                    PHÒNG KHÁM ĐA KHOA
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Body Content -->
                    <tr>
                        <td style='padding:45px 35px; text-align:center'>
                            <!-- Icon -->
                            <div style='width:80px; height:80px; background:linear-gradient(135deg, #4a6cf7 0%, #7b4af7 100%); border-radius:50%; margin:0 auto 25px; display:flex; align-items:center; justify-content:center; box-shadow:0 8px 25px rgba(74, 108, 247, 0.3)'>
                                <svg width='36' height='36' viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                                    <path d='M21 6L9 18L4 13' stroke='white' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round'/>
                                </svg>
                            </div>
                            
                            <h2 style='color:#1a1a1a; margin:0 0 15px; font-size:26px; font-weight:600'>
                                Mã xác minh của bạn
                            </h2>
                            <p style='color:#666; margin:0 0 25px; font-size:16px; line-height:1.6'>
                                Xin chào <strong style='color:#4a6cf7'>{user.FullName}</strong>,<br>
                                Vui lòng sử dụng mã OTP dưới đây để hoàn tất xác thực tài khoản
                            </p>
                            
                            <!-- OTP Container -->
                            <div style='background:linear-gradient(135deg, #f8f9ff 0%, #f0f4ff 100%); border:2px dashed #4a6cf7; border-radius:16px; padding:28px; margin:35px 0; position:relative'>
                                <div style='position:absolute; top:-12px; left:50%; transform:translateX(-50%); background:white; padding:0 15px; font-size:14px; color:#4a6cf7; font-weight:600'>
                                    MÃ OTP
                                </div>
                                <div style='font-size:48px; font-weight:800; letter-spacing:15px; color:#4a6cf7; margin:10px 0 0; text-align:center; padding-left:15px'>
                                    {otp}
                                </div>
                            </div>
                            
                            <!-- Timer Warning -->
                            <div style='background:#fff5f5; border:1px solid #fed7d7; border-radius:12px; padding:16px; margin:25px 0'>
                                <p style='color:#e53e3e; margin:0; font-size:14px; font-weight:500'>
                                    ⏰ Mã sẽ hết hạn sau <strong>3 phút</strong>
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Security Footer -->
                    <tr>
                        <td style='background:linear-gradient(135deg, #f8f9ff 0%, #f0f4ff 100%); padding:35px 30px; text-align:center; border-top:1px solid #e8f0fe'>
                            <!-- Security Icon -->
                            <div style='margin-bottom:20px'>
                                <svg width='48' height='48' viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                                    <path d='M12 22C12 22 20 18 20 12V5L12 2L4 5V12C4 18 12 22 12 22Z' stroke='#4a6cf7' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
                                    <path d='M9 12L11 14L15 10' stroke='#4a6cf7' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
                                </svg>
                            </div>
                            
                            <p style='color:#666; margin:0 0 20px; font-size:14px; line-height:1.6; font-weight:500'>
                                🔒 Bảo mật quan trọng
                            </p>
                            <p style='color:#888; margin:0 0 25px; font-size:13px; line-height:1.5'>
                                Không chia sẻ mã này với bất kỳ ai, kể cả nhân viên VITACARE.<br>
                                Mã OTP chỉ được sử dụng cho mục đích xác thực tài khoản.
                            </p>
                            
                            <div style='border-top:1px solid #e8f0fe; padding-top:25px'>
                                <p style='color:#999; font-size:12px; margin:0; line-height:1.4'>
                                    © 2025 VITACARE - Phòng khám đa khoa<br>
                                    Tất cả quyền được bảo lưu.
                                </p>
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            await _emailService.SendEmailAsync(
                user.Email,
                "[PHÒNG KHÁM VITACARE] MÃ OTP XÁC THỰC TÀI KHOẢN",
                emailBody
            );

            return Ok("OTP đã được gửi đến email.");
        }

        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOtpRequest request)
        {
            var user = await _userService.SingleOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return NotFound("Email không tồn tại");

            var isValid = await _otpService.VerifyOtpAsync(user.UserId, request.OTP);
            if (!isValid)
                return BadRequest("OTP không hợp lệ hoặc đã hết hạn");

            return Ok("Xác thực OTP thành công");
        }

        //api cho reset password
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _userService.GetByEmailAsync(request.Email);
            if (user == null)
                return NotFound("Email không tồn tại");

            string newPassword = Guid.NewGuid().ToString("N")[..8];

            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                user.PasswordHash = PasswordHelper.HashPassword(newPassword);
                user.MustChangePassword = true;

                await _userRepository.Update(user);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                //gửi email xác nhận mk mới
                string bodyEmail =
                    $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Mật khẩu mới - VITACARE</title>
    <link href='https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap' rel='stylesheet'>
</head>
<body style='margin:0; padding:0; background:linear-gradient(135deg, #f5f7fa 0%, #e4edf5 100%); font-family:Inter,Arial,sans-serif'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background:linear-gradient(135deg, #f5f7fa 0%, #e4edf5 100%); padding:40px 0'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' style='max-width:520px; background:#ffffff; border-radius:20px; overflow:hidden; box-shadow:0 15px 40px rgba(74, 108, 247, 0.15); border:1px solid #e8f0fe'>
                    <!-- Header -->
                    <tr>
                        <td style='background:linear-gradient(135deg, #4a6cf7 0%, #7b4af7 100%); padding:45px 30px; text-align:center; position:relative'>
                            <div style='position:absolute; top:0; left:0; right:0; bottom:0; background-color:rgba(255,255,255,0.1);'></div>
                            <div style='position:relative; z-index:1'>
                                <h1 style='color:white; margin:0; font-size:32px; font-weight:700; letter-spacing:-0.5px'>
                                    VITACARE
                                </h1>
                                <p style='color:rgba(255,255,255,0.95); margin:12px 0 0; font-size:16px; font-weight:400'>
                                    PHÒNG KHÁM ĐA KHOA
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Body Content -->
                    <tr>
                        <td style='padding:45px 35px; text-align:center'>
                            
                            <h2 style='color:#1a1a1a; margin:0 0 15px; font-size:26px; font-weight:600'>
                                Mật khẩu mới của bạn
                            </h2>
                            <p style='color:#666; margin:0 0 25px; font-size:16px; line-height:1.6'>
                                Xin chào <strong style='color:#4a6cf7'>{user.FullName}</strong>,<br>
                                Yêu cầu cấp lại mật khẩu của bạn đã được xử lý thành công
                            </p>
                            
                            <!-- Password Container -->
                            <div style='background:linear-gradient(135deg, #f8f9ff 0%, #f0f4ff 100%); border:2px dashed #4a6cf7; border-radius:16px; padding:28px; margin:35px 0; position:relative'>
                                <div style='position:absolute; top:-12px; left:50%; transform:translateX(-50%); background:white; padding:0 15px; font-size:14px; color:#4a6cf7; font-weight:600'>
                                    MẬT KHẨU MỚI
                                </div>
                                <div style='font-size:32px; font-weight:800; letter-spacing:4px; color:#4a6cf7; margin:10px 0 0; text-align:center; font-family:monospace'>
                                    {newPassword}
                                </div>
                            </div>
                            
                            <!-- Important Notice -->
                            <div style='background:#fff5f5; border:1px solid #fed7d7; border-radius:12px; padding:20px; margin:25px 0; text-align:left'>
                                <h3 style='color:#e53e3e; margin:0 0 12px; font-size:16px; font-weight:600'>
                                    ⚠️ Lưu ý quan trọng
                                </h3>
                                <ul style='color:#e53e3e; margin:0; padding-left:20px; font-size:14px; line-height:1.5'>
                                    <li>Vui lòng đăng nhập ngay với mật khẩu mới này</li>
                                    <li>Hệ thống sẽ yêu cầu bạn đổi mật khẩu sau khi đăng nhập</li>
                                    <li>Không chia sẻ mật khẩu này với bất kỳ ai</li>
                                </ul>
                            </div>
                            
                            <!-- Action Button -->
                            <div style='margin:30px 0 20px'>
                                <a href='#' style='display:inline-block; background:linear-gradient(135deg, #4a6cf7 0%, #7b4af7 100%); color:white; padding:14px 32px; text-decoration:none; border-radius:8px; font-weight:600; font-size:16px; box-shadow:0 4px 15px rgba(74, 108, 247, 0.3)'>
                                    Đăng nhập ngay
                                </a>
                            </div>
                            
                            <p style='color:#888; margin:0; font-size:14px; line-height:1.5'>
                                Nếu bạn không yêu cầu cấp lại mật khẩu,<br>
                                vui lòng liên hệ với chúng tôi ngay lập tức.
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Security Footer -->
                    <tr>
                        <td style='background:linear-gradient(135deg, #f8f9ff 0%, #f0f4ff 100%); padding:35px 30px; text-align:center; border-top:1px solid #e8f0fe'>
                            <!-- Security Icon -->
                            <div style='margin-bottom:20px'>
                                <svg width='48' height='48' viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                                    <path d='M12 22C12 22 20 18 20 12V5L12 2L4 5V12C4 18 12 22 12 22Z' stroke='#4a6cf7' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
                                    <path d='M9 12L11 14L15 10' stroke='#4a6cf7' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>
                                </svg>
                            </div>
                            
                            <p style='color:#666; margin:0 0 20px; font-size:14px; line-height:1.6; font-weight:500'>
                                🔒 Bảo mật tài khoản
                            </p>
                            <p style='color:#888; margin:0 0 25px; font-size:13px; line-height:1.5'>
                                Để bảo vệ tài khoản của bạn, vui lòng:<br>
                                • Đổi mật khẩu ngay sau khi đăng nhập<br>
                                • Không sử dụng lại mật khẩu cũ<br>
                                • Bật xác thực 2 yếu tố nếu có thể
                            </p>
                            
                            <div style='border-top:1px solid #e8f0fe; padding-top:25px'>
                                <p style='color:#999; font-size:12px; margin:0; line-height:1.4'>
                                    © 2025 VITACARE - Phòng khám đa khoa<br>
                                    Tất cả quyền được bảo lưu.
                                </p>
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

                await _emailService.SendEmailAsync(
                    user.Email,
                    "[PHÒNG KHÁM VITACARE] MẬT KHẨU MỚI - VUI LÒNG KIỂM TRA NGAY",
                    bodyEmail
                );

                return Ok("Mật khẩu mới đã được gửi đến email của bạn.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        //vô hiệu hóa tài khoản

        [HttpPut("DeactivateAccount")]
        public async Task<IActionResult> DeactivateAccount()
        {
            var usernameClaim = User.FindFirst("username");
            if (usernameClaim == null)
                return Unauthorized("Không xác định được người dùng từ token.");

            var username = usernameClaim.Value;

            var result = await _userService.DeactivateAccountAsync(username);
            
            return Ok("Tài khoản đã bị vô hiệu hóa thành công.");
        }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
    }
}
