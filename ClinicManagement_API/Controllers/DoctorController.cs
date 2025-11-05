using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [Authorize(Roles = "Doctor")]
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IMedicineService _medicineService;

        public DoctorController(IDoctorService doctorService, IMedicineService medicineService)
        {
            _doctorService = doctorService;
            _medicineService = medicineService;
        }

        [HttpGet("current-room")]
        public async Task<IActionResult> GetCurrentRoom()
        {
            var staffId = int.Parse(User.FindFirst("userid")!.Value);
            var roomId = await _doctorService.GetCurrentRoomId(staffId);
            return Ok(new { roomId });
        }

        [HttpGet("GetMySchedule")]
        public async Task<
            ActionResult<ResponseValue<List<ScheduleForMedicalStaffResponse>>>
        > GetMySchedule()
        {
            var staffIdClaim = User.FindFirst("userid")?.Value;
            if (!int.TryParse(staffIdClaim, out int staffId))
            {
                return new ResponseValue<List<ScheduleForMedicalStaffResponse>>(
                    null,
                    StatusReponse.Unauthorized,
                    "Unauthorized"
                );
            }
            try
            {
                var schedules = await _doctorService.GetAllMySchedulesAsync(
                    int.Parse(staffIdClaim)
                );
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return new ResponseValue<List<ScheduleForMedicalStaffResponse>>(
                    null,
                    StatusReponse.Error,
                    ex.Message
                );
            }
        }

        [HttpGet("GetAllMedicinesByDoctorAsync")]
        public async Task<ResponseValue<PagedResult<MedicineDTO>>> GetAllMedicinesByDoctorAsync(
            [FromQuery] string? search = null
        )
        {
            try
            {
                var result = await _doctorService.GetAllMedicinesAsync(search);

                return new ResponseValue<PagedResult<MedicineDTO>>(
                    result.Content,
                    StatusReponse.Success,
                    "Lấy danh sách thuốc thành công."
                );
            }
            catch (ArgumentException ex)
            {
                return new ResponseValue<PagedResult<MedicineDTO>>(
                    null,
                    StatusReponse.BadRequest,
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                return new ResponseValue<PagedResult<MedicineDTO>>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi lấy danh sách thuốc: " + ex.Message
                );
            }
        }

        [HttpGet("GetAllServicesByDoctorAsync")]
        public async Task<
            ActionResult<ResponseValue<PagedResult<ServiceDTO>>>
        > GetAllServicesByDoctorAsync(string? search = null)
        {
            try
            {
                var result = await _doctorService.GetAllServicesAsync(search);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Đã xảy ra lỗi khi lấy danh sách người dùng. Vui lòng thử lại sau.",
                    }
                );
            }
        }

        [HttpGet("GetTodaysAppointmentsAsync")]
        public async Task<ResponseValue<List<TodaysAppointmentDTO>>> GetTodaysAppointmentsAsync(
            [FromQuery] DateOnly date
        )
        {
            try
            {
                var appointments = await _doctorService.GetTodaysAppointmentsAsync(date);
                return new ResponseValue<List<TodaysAppointmentDTO>>(
                    appointments,
                    StatusReponse.Success,
                    "Lấy dữ liệu thành công"
                );
            }
            catch (Exception ex)
            {
                return new ResponseValue<List<TodaysAppointmentDTO>>(
                    null,
                    StatusReponse.Error,
                    ex.Message
                );
            }
        }

        [HttpPost("SubmitExamination")]
        public async Task<ActionResult<ResponseValue<object>>> SubmitExamination(
            [FromBody] ExaminationRequestDto request
        )
        {
            var staffIdClaim = User.FindFirst("userid")?.Value;
            if (!int.TryParse(staffIdClaim, out int staffId))
            {
                return Unauthorized(
                    new ResponseValue<object>(null, StatusReponse.Unauthorized, "Unauthorized")
                );
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _doctorService.SubmitExaminationAsync(request, staffId);

            if (result.Status != StatusReponse.Success)
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }

        [HttpGet("my-queue-today")]
        public async Task<IActionResult> GetMyQueueTodayAsync()
        {
            var staffIdClaim = int.Parse(User.FindFirst("userid")?.Value);
            var result = await _doctorService.GetMyQueueTodayAsync(staffIdClaim);
            return Ok(new { success = true, data = result });
        }
    }
}
