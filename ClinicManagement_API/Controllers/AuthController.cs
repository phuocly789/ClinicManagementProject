using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement_API.Models;
using ClinicManagement_Infrastructure.Data.Models;
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

        public AuthController(
            IUserRepository userRepository,
            IUserService userService,
            JwtAuthService jwtAuthService,
            IPatinetService patientService,
            IOtpService otpService,
            IEmailService emailService
        )
        {
            _userRepository = userRepository;
            _userService = userService;
            _jwtAuthService = jwtAuthService;
            _patientService = patientService;
            _otpService = otpService;
            _emailService = emailService;
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
    <link href='https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap' rel='stylesheet'>
</head>
<body style='margin:0; padding:0; background:#f4f7fa; font-family:Inter,Arial,sans-serif'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f7fa; padding:30px 0'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' style='max-width:480px; background:#ffffff; border-radius:16px; overflow:hidden; box-shadow:0 10px 30px rgba(0,0,0,0.08)'>
                    <!-- Header -->
                    <tr>
                        <td style='background:linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding:40px 30px; text-align:center'>
                            <h1 style='color:white; margin:0; font-size:28px; font-weight:700; letter-spacing:-0.5px'>
                                VITACARE
                            </h1>
                            <p style='color:rgba(255,255,255,0.9); margin:12px 0 0; font-size:16px'>
                                PHÒNG KHÁM ĐA KHOA
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Body -->
                    <tr>
                        <td style='padding:40px 30px; text-align:center'>
                            <h2 style='color:#1a1a1a; margin:0 0 20px; font-size:22px; font-weight:600'>
                                Mã xác minh của bạn
                            </h2>
                            <p style='color:#666; margin:0 0 32px; font-size:16px; line-height:1.6'>
                                Xin chào <strong>{user.FullName}</strong>,<br>
                                Vui lòng sử dụng mã dưới đây để hoàn tất xác thực
                            </p>
                            
                            <!-- OTP Box -->
                            <div style='background:#f8f9ff; border:2px dashed #667eea; border-radius:16px; padding:24px; margin:32px 0'>
                                <div style='font-size:42px; font-weight:800; letter-spacing:12px; color:#667eea; margin:0'>
                                    {otp}
                                </div>
                            </div>
                            
                            <p style='color:#888; margin:0; font-size:14px'>
                                Mã sẽ hết hạn sau <strong style='color:#d32f2f'>3 phút</strong>
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='background:#f8f9ff; padding:30px; text-align:center'>
                            <p style='color:#999; margin:0; font-size:13px; line-height:1.6'>
                                Bảo mật: Không chia sẻ mã này với bất kỳ ai, kể cả nhân viên VITACARE.<br>
                                Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email.
                            </p>
                            <div style='margin-top:24px'>
                                <p style='color:#ccc; font-size:12px; margin:0'>
                                    © 2025 VITACARE. Tất cả quyền được bảo lưu.
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

        // [HttpPost("ResetPassword")]
        // public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        // {
        //     var result = await _userService.ChangePasswordAsync(request.Email, request.NewPassword);
        //     return Ok(result);
        // }
    }
}
