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
    public class SupplierController : ControllerBase
    {
        private readonly ILogger<SupplierController> _logger;
        private readonly ISuplierService _suplierService;

        public SupplierController(
            ILogger<SupplierController> logger,
            ISuplierService suplierService
        )
        {
            _suplierService = suplierService;
            _logger = logger;
        }

        [HttpGet("GetAllSupplierAsync")]
        public async Task<ResponseValue<PagedResult<SuplierDTO>>> GetAllSupplierAsync(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var rusult = await _suplierService.GetAllMSupliersAsync(search, page, pageSize);

                return new ResponseValue<PagedResult<SuplierDTO>>(
                    rusult.Content,
                    StatusReponse.Success,
                    "Lấy danh sách thuốc thành công."
                );
            }
            catch (ArgumentException ex)
            {
                return new ResponseValue<PagedResult<SuplierDTO>>(
                    null,
                    StatusReponse.BadRequest,
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                return new ResponseValue<PagedResult<SuplierDTO>>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi lấy danh sách thuốc: " + ex.Message
                );
            }
        }

        [HttpPost("CreateSupplierAsync")]
        public async Task<ActionResult<ResponseValue<SuplierDTO>>> CreateSupplierAsync(
            [FromBody] SuplierDTO request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _suplierService.CreateSuplierAsync(request);
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

        [HttpPut("UpdateSupplierAsync/{id}")]
        public async Task<ActionResult<ResponseValue<SuplierDTO>>> UpdateSupplierAsync(
            int id,
            [FromBody] SuplierDTO request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _suplierService.UpdateSuplierAsync(id, request);
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

        [HttpDelete("DeleteSupplierAsync/{id}")]
        public async Task<IActionResult> DeleteSupplierAsync(int id)
        {
            try
            {
                await _suplierService.DeleteSuplierAsync(id);
                return Ok(new { success = true, message = "Xóa thuốc thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = "Không tìm thấy thuốc" });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Đã xảy ra lỗi khi xóa thuốc. Vui lòng thử lại sau.",
                    }
                );
            }
        }
    }
}
