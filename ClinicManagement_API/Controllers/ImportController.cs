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
    public class ImportController : ControllerBase
    {
        private readonly ILogger<ImportController> _logger;
        private readonly IImportService _importService;

        public ImportController(ILogger<ImportController> logger, IImportService importService)
        {
            _logger = logger;
            _importService = importService;
        }

        [HttpGet("GetAllImportBillsAsync")]
        public async Task<ResponseValue<PagedResult<ImportDTO>>> GetAllImportBillsAsync()
        {
            var result = await _importService.GetAllImportBillsAsync();
            return new ResponseValue<PagedResult<ImportDTO>>(
                result.Content,
                StatusReponse.Success,
                "Lấy danh sách nhập hàng thành công."
            );
        }

        [HttpPost("CreateImportBillAsync")]
        public async Task<ActionResult<ResponseValue<ImportDTO>>> CreateImportBillAsync(
            [FromBody] ImportCreateDTO request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _importService.CreateImportBillAsync(request);
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

        [HttpGet("GetImportBillByIdAsync/{id}")]
        public async Task<ResponseValue<ImportDetailByIdDTO>> GetImportBillByIdAsync(int id)
        {
            try
            {
                var result = await _importService.GetImportBillByIdAsync(id);
                if (result.Status == StatusReponse.Success)
                {
                    return new ResponseValue<ImportDetailByIdDTO>(
                        result.Content,
                        StatusReponse.Success,
                        "Lấy nhập hàng thành công."
                    );
                }
                else if (result.Status == StatusReponse.NotFound)
                {
                    return new ResponseValue<ImportDetailByIdDTO>(
                        null,
                        StatusReponse.NotFound,
                        "Không tìm thấy nhập hàng."
                    );
                }
                else
                {
                    return new ResponseValue<ImportDetailByIdDTO>(
                        null,
                        StatusReponse.Error,
                        "Đã xảy ra lỗi khi lấy nhập hàng: " + result.Message
                    );
                }
            }
            catch (Exception ex)
            {
                return new ResponseValue<ImportDetailByIdDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi lấy nhập hàng: " + ex.Message
                );
            }
        }
    }
}
