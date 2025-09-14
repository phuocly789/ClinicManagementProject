using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public AuthController(
            IUserRepository userRepository,
            IUserService userService,
            JwtAuthService jwtAuthService
        )
        {
            _userRepository = userRepository;
            _userService = userService;
            _jwtAuthService = jwtAuthService;
        }

        [HttpGet("UserLogin")]
        public async Task<ActionResult> UserLogin([FromBody] UserLoginDTO loginDto)
        {
            if (loginDto == null)
                return BadRequest("Invalid login data.");
            var (success, message) = await _userService.Login(loginDto);
            if (!success)
            {
                if (
                    message == MessageLogin.UserNameOrEmailRequired
                    || message == MessageLogin.PasswordRequired
                )
                    return BadRequest(message);

                if (
                    message == MessageLogin.UserNotFound
                    || message == MessageLogin.InvalidCredentials
                )
                    return Unauthorized(message);

                return StatusCode(500, message);
            }

            // message là token nếu đăng nhập thành công
            return Ok(message);
        }
    }
}
