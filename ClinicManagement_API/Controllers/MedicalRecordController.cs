using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;
        public MedicalRecordController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMedicalRecordsAsync()
        {
            try
            {
                var result = await _medicalRecordService.GetAllMedicalRecordsAsync();
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
                        message = "Có lỗi xảy ra khi lấy danh sách hồ sơ bệnh án.",
                        error = ex.Message
                    });
            }
        }

        [Authorize(Roles = "Receptionist")]
        [HttpPost]
        public async Task<ActionResult<ResponseValue<MedicalRecordDTO>>> MedicalRecordCreateAsync([FromBody] MedicalRecordDTO request)
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

            var result = await _medicalRecordService.MedicalRecordCreateAsync(request, createdBy);

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
    }
}