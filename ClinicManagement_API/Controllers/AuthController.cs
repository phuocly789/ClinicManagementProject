using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement_API.Models;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;

//using ClinicManagement_API.Models;

namespace ClinicManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IUserRepository _userRepository;
        private readonly IPatinetService _patientService;

        public AuthController(
            IUserRepository userRepository,
            IUserService userService,
            JwtAuthService jwtAuthService,
            IPatinetService patientService
        )
        {
            _userRepository = userRepository;
            _userService = userService;
            _jwtAuthService = jwtAuthService;
            _patientService = patientService;
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
    }
}
