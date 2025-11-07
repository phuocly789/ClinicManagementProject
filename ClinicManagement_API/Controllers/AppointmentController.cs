
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetAppointmentsAsync(int staffId, [FromQuery] DateOnly? date = null)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentsAsync(staffId, date);
                return Ok(
                    new
                    {
                        success = true,
                        data = result
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Có lỗi xảy ra khi lấy danh sách lịch hẹn.",
                        error = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Receptionist, Patient")]
        [HttpPost]
        public async Task<ActionResult<ResponseValue<AppointmentDTO>>> CreateAppointmentAsync([FromBody] AppointmentCreateDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc thiếu UserId." });
            }

            int createdBy = int.Parse(userIdClaim);

            var result = await _appointmentService.AddToAppointmentAsync(request, createdBy);

            if (result.Status == StatusReponse.Success)
            {
                return Created("", result);
            }
            else if (result.Status == StatusReponse.BadRequest)
            {
                return BadRequest(result);
            }
            else if (result.Status == StatusReponse.Unauthorized)
            {
                return StatusCode(403, result);
            }
            return StatusCode(500, result);
        }


        [Authorize(Roles = "Receptionist")]
        [HttpPut("AppointmentUpdateStatusAsync/{id}")]
        public async Task<ActionResult<ResponseValue<AppointmentDTO>>> AppointmentUpdateStatusAsync(int id, [FromBody] AppointmentStatusDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _appointmentService.AppointmentUpdateStatusAsync(id, request);

            if (result.Status == StatusReponse.Success)
            {
                return Ok(result);
            }
            else if (result.Status == StatusReponse.NotFound)
            {
                return NotFound(result);
            }
            else if (result.Status == StatusReponse.Unauthorized)
            {
                return StatusCode(403, result);
            }
            return StatusCode(500, result);
        }

        [Authorize(Roles = "Patient")]
        [HttpPut("AppointmentCancelAsync/{id}")]
        public async Task<ActionResult<ResponseValue<AppointmentDTO>>> AppointmentCancelAsync(int id, [FromBody] AppointmentStatusDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _appointmentService.AppointmentUpdateStatusAsync(id, request);

            if (result.Status == StatusReponse.Success)
            {
                return Ok(result);
            }
            else if (result.Status == StatusReponse.NotFound)
            {
                return NotFound(result);
            }
            else if (result.Status == StatusReponse.Unauthorized)
            {
                return StatusCode(403, result);
            }
            return StatusCode(500, result);
        }
    }
}