using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatinetService _patinetService;

        public PatientController(IPatinetService patinetService)
        {
            _patinetService = patinetService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPatientsAsync()
        {
            try
            {
                var result = await _patinetService.GetAllPatientsAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Có lỗi xảy ra khi lấy danh sách bệnh nhân.",
                        error = ex.Message,
                    }
                );
            }
        }

        [HttpGet("patientId")]
        public async Task<IActionResult> GetPatientByIdAsync(int id)
        {
            try
            {
                var result = await _patinetService.GetPatientByIdAsync(id);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Có lỗi xảy ra khi lấy bệnh nhân.",
                        error = ex.Message,
                    }
                );
            }
        }
    }
}
