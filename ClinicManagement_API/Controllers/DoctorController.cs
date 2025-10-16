using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    // [Authorize(Roles = "Doctor")]
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet("GetMySchedule")]
        public async Task<ResponseValue<List<AppointmentMyScheduleDto>>> GetMySchedule(
            int staffId,
            [FromQuery] DateOnly? date = null
        )
        {
            try
            {
                var appointments = await _doctorService.GetAppointmentsByStaffIdAnddDateAsync(
                    staffId,
                    date
                );
                return new ResponseValue<List<AppointmentMyScheduleDto>>(
                    appointments,
                    StatusReponse.Success,
                    "Lấy dữ liệu thành công"
                );
            }
            catch (Exception ex)
            {
                return new ResponseValue<List<AppointmentMyScheduleDto>>(
                    null,
                    StatusReponse.Error,
                    ex.Message
                );
            }
        }

        [HttpPost("CreateDiagnosisAsync")]
        public async Task<ResponseValue<DiagnosisDataDto>> CreateDiagnosisAsync(
            [FromBody] CreateDiagnosisDto request,
            int currentStaffId
        )
        {
            try
            {
                var diagnosis = await _doctorService.CreateDiagnosisAsync(request, currentStaffId);
                return new ResponseValue<DiagnosisDataDto>(
                    diagnosis,
                    StatusReponse.Success,
                    "Tạo chuẩn đoán thành công"
                );
            }
            catch (Exception ex)
            {
                return new ResponseValue<DiagnosisDataDto>(null, StatusReponse.Error, ex.Message);
            }
        }

        [HttpPost("CreateServiceOrderAsync")]
        public async Task<
            ActionResult<ResponseValue<ServiceOrderResponseDto>>
        > CreateServiceOrderAsync([FromBody] CreateServiceOrderDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(
                    new ResponseValue<ServiceOrderResponseDto>(
                        null,
                        StatusReponse.Unauthorized,
                        "Unauthorized"
                    )
                );
            }
            var result = await _doctorService.CreateServiceOrderAsync(request, currentUserId);
            if (!(result.Status == StatusReponse.Success))
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("CreatePrescriptionAsync")]
        public async Task<
            ActionResult<ResponseValue<PrescriptionResponseDto>>
        > CreatePrescriptionAsync([FromBody] PrescriptionRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(
                    new ResponseValue<PrescriptionResponseDto>(
                        null,
                        StatusReponse.Unauthorized,
                        "Unauthorized"
                    )
                );
            }
            var result = await _doctorService.CreatePrescriptionAsync(request, currentUserId);
            if (!(result.Status == StatusReponse.Success))
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("GetAppointmentResultsAsync/{appointmentId}")]
        public async Task<
            ActionResult<ResponseValue<ServiceOrderResultDto>>
        > GetAppointmentResultsAsync(int appointmentId)
        {
            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(
                    new ResponseValue<ServiceOrderResultDto>(
                        null,
                        StatusReponse.Unauthorized,
                        "Unauthorized"
                    )
                );
            }
            var result = await _doctorService.GetAppointmentResultsAsync(
                appointmentId,
                currentUserId
            );
            if (!(result.Status == StatusReponse.Success))
            {
                return BadRequest(result);
            }
            return Ok(result);
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
    }
}
