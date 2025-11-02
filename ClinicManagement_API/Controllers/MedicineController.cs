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
    public class MedicineController : ControllerBase
    {
        private readonly ILogger<MedicineController> _logger;
        private readonly IMedicineService _medicineService;

        public MedicineController(
            IMedicineService medicineService,
            ILogger<MedicineController> logger
        )
        {
            _medicineService = medicineService;
            _logger = logger;
        }

        [HttpGet("GetAllMedicinesAsync")]
        public async Task<ResponseValue<PagedResult<MedicineDTO>>> GetAllMedicinesAsync(
            [FromQuery] string? search = null,
            [FromQuery] string? type = null, // Thêm
            [FromQuery] string? unit = null, // Thêm
            [FromQuery] decimal? minPrice = null, // Thêm
            [FromQuery] decimal? maxPrice = null, // Thêm
            [FromQuery] bool lowStock = false, // Thêm
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var result = await _medicineService.GetAllMedicinesAsync(
                    search,
                    type,
                    unit,
                    minPrice,
                    maxPrice,
                    lowStock,
                    page,
                    pageSize
                );

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

        [HttpGet("GetMedicineByIdAsync/{id}")]
        public async Task<ResponseValue<MedicineDTO>> GetMedicineByIdAsync(int id)
        {
            try
            {
                var result = await _medicineService.GetMedicineByIdAsync(id);
                if (result.Status == StatusReponse.Success)
                {
                    return new ResponseValue<MedicineDTO>(
                        result.Content,
                        StatusReponse.Success,
                        "Lấy thuốc thành công."
                    );
                }
                else if (result.Status == StatusReponse.NotFound)
                {
                    return new ResponseValue<MedicineDTO>(
                        null,
                        StatusReponse.NotFound,
                        "Không tìm thấy thuốc."
                    );
                }
                else
                {
                    return new ResponseValue<MedicineDTO>(
                        null,
                        StatusReponse.Error,
                        "Đã xảy ra lỗi khi lấy thuốc: " + result.Message
                    );
                }
            }
            catch (Exception ex)
            {
                return new ResponseValue<MedicineDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi lấy thuốc: " + ex.Message
                );
            }
        }

        [HttpPost("CreateMedicineAsync")]
        public async Task<ActionResult<ResponseValue<MedicineDTO>>> CreateMedicineAsync(
            [FromBody] MedicineDTO request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _medicineService.CreateMedicineAsync(request);
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

        [HttpPut("UpdateMedicineAsync/{id}")]
        public async Task<ActionResult<ResponseValue<MedicineDTO>>> UpdateMedicineAsync(
            int id,
            [FromBody] MedicineDTO request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _medicineService.UpdateMedicineAsync(id, request);
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

        [HttpDelete("DeleteMedicineAsync/{id}")]
        public async Task<IActionResult> DeleteMedicineAsync(int id)
        {
            try
            {
                await _medicineService.DeleteMedicineAsync(id);
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

        [HttpGet("Inventory-Warnings")]
        public async Task<ResponseValue<List<LowStockMedicineDTO>>> GetLowStockMedicinesAsync(
            [FromQuery] int threshold = 20
        )
        {
            try
            {
                var result = await _medicineService.GetLowStockMedicinesAsync(threshold);
                return new ResponseValue<List<LowStockMedicineDTO>>(
                    result,
                    StatusReponse.Success,
                    "Lấy danh sách thuốc thành công."
                );
            }
            catch (Exception ex)
            {
                return new ResponseValue<List<LowStockMedicineDTO>>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi lấy danh sách thuốc: " + ex.Message
                );
            }
        }
    }
}
