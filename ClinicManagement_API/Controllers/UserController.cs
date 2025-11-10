using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement_API.Models;
using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPatinetService _patientService;

        public UserController(IUserService userService, IPatinetService patientService)
        {
            _userService = userService;
            _patientService = patientService;
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

        //GetByEmailAsync
        [HttpGet("GetByEmail/{email}")]
        public async Task<ActionResult> GetByEmail(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        //ChangePasswordAsync
        [HttpPut("ChangePassword/{username}")]
        public async Task<ActionResult> ChangePassword(
            string username,
            [FromBody] ChangePasswordRequest request
        )
        {
            var result = await _userService.ChangePasswordAsync(
                username,
                request.CurrentPassword,
                request.NewPassword
            );
            return Ok(result);
        }

        //UpdateUserAsync
        [HttpPut("UpdateUser/{username}")]
        public async Task<ActionResult> UpdateUser(string username, UserUpdateDTO request)
        {
            var result = await _userService.UpdateUserAsync(request, username);
            return Ok(result);
        }

        //lấy lịch khám theo khung giờ
        [HttpGet("GetAvailableTimeSlots/{date}")]
        public async Task<ActionResult> GetAvailableTimeSlots(DateOnly date)
        {
            var result = await _patientService.GetAvailableTimeSlotsAsync(date);
            return Ok(result);
        }

        //create appointment
        [HttpPost("CreateAppointmentByPatient")]
        public async Task<ActionResult<ResponseValue<AppointmentDTO>>> CreateAppointmentByPatient(
            [FromBody] AppointmentDTO request
        )
        {
            var patientId = int.Parse(User.FindFirst("userid")!.Value);
            var result = await _patientService.CreateAppointmentByPatientAsync(request, patientId);
            if (result.Status == "Success")
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        //get my appoiment
        [HttpGet("GetMyAppointment")]
        public async Task<ActionResult> GetMyAppointment()
        {
            var patientId = int.Parse(User.FindFirst("userid")!.Value);
            var result = await _patientService.GetMyAppointmentAsync(patientId);
            if (result.Status == "Success")
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}
