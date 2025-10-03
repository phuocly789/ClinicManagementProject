using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//using ClinicManagement_API.Models;

namespace ClinicManagement_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(IServiceService serviceService, ILogger<ServiceController> logger)
        {
            _serviceService = serviceService;
            _logger = logger;
        }

        [HttpGet("GetAllServicesAsync")]
        public async Task<IActionResult> GetAllServicesAsync(
            [FromQuery] string search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var result = await _serviceService.GetAllServicesAsync(search, page, pageSize);
                return Ok(
                    new
                    {
                        success = true,
                        data = new
                        {
                            result.TotalItems,
                            result.Page,
                            result.PageSize,
                            services = result.Items,
                        },
                    }
                );
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

        [HttpGet("GetServiceByIdAsync/{id}")]
        public async Task<ResponseValue<ServiceDTO>> GetServiceByIdAsync(int id)
        {
            try
            {
                var result = await _serviceService.GetServiceByIdAsync(id);
                if (result.Status == StatusReponse.Success)
                {
                    return new ResponseValue<ServiceDTO>(
                        result.Content,
                        StatusReponse.Success,
                        "Lấy dịch vụ thành công."
                    );
                }
                else if (result.Status == StatusReponse.NotFound)
                {
                    return new ResponseValue<ServiceDTO>(
                        null,
                        StatusReponse.NotFound,
                        "Không tìm thấy dịch vụ."
                    );
                }
                else
                {
                    return new ResponseValue<ServiceDTO>(
                        null,
                        StatusReponse.Error,
                        "Đã xảy ra lỗi khi lấy dịch vụ: " + result.Message
                    );
                }
            }
            catch (Exception ex)
            {
                return new ResponseValue<ServiceDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi lấy dịch vụ: " + ex.Message
                );
            }
        }

        [HttpPost("CreateServiceAsync")]
        public async Task<ActionResult<ResponseValue<ServiceDTO>>> CreateServiceAsync(
            [FromBody] ServiceDTO request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _serviceService.CreateServiceAsync(request);
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

        [HttpPut("UpdateServiceAsync/{id}")]
        public async Task<ActionResult<ResponseValue<ServiceDTO>>> UpdateServiceAsync(
            int id,
            [FromBody] ServiceDTO request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _serviceService.UpdateServiceAsync(id, request);
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

        [HttpDelete("DeleteServiceAsync/{id}")]
        public async Task<IActionResult> DeleteServiceAsync(int id)
        {
            try
            {
                await _serviceService.DeleteServiceAsync(id);
                return Ok(new { success = true, message = "Xóa dịch vụ thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = "Không tìm thấy dịch vụ" });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Đã xảy ra lỗi khi xóa dịch vụ. Vui lòng thử lại sau.",
                    }
                );
            }
        }
    }
}
