using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement_API.Models;
using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("GetAllUsers")]
        public async Task<ActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();
            var userDtos = users.Select(u => new UserDTO
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Gender = u.Gender,
                Address = u.Address,
                DateOfBirth = u.DateOfBirth,
                IsActive = u.IsActive,
            });
            return Ok(userDtos);
        }

        //GetByUsernameAsync
        [HttpGet("GetByUsername/{username}")]
        public async Task<ActionResult> GetByUsername(string username)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
    }
}
