using System;
using System.Threading.Tasks;
using ClinicManagement_Infrastructure.Data.Models;
// using ClinicManagement_Infrastructure.Infrastructure.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // Chỉ Admin có quyền truy cập
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportService;

        public ReportsController(IReportsService reportService)
        {
            _reportService = reportService;
        }

        // API 22: GET /api/reports/visits?startDate={startDate}&endDate={endDate}
        [HttpGet("GetVisitStatistics")]
        public async Task<ActionResult<ResponseValue<VisitReportDTO>>> GetVisitStatistics(
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null
        )
        {
            var result = await _reportService.GetVisitStatisticsAsync(startDate, endDate);
            if (result.Status == StatusReponse.Success)
            {
                return Ok(new { success = true, data = result.Content });
            }
            else if (result.Status == StatusReponse.BadRequest)
            {
                return BadRequest(new { success = false, message = result.Message });
            }
            else if (result.Status == StatusReponse.Unauthorized)
            {
                return StatusCode(403, new { success = false, message = result.Message });
            }
            return StatusCode(500, new { success = false, message = result.Message });
        }

        // API 23: GET /api/reports/revenue?startDate={startDate}&endDate={endDate}
        [HttpGet("GetRevenueStatistics")]
        public async Task<ActionResult<ResponseValue<RevenueReportDTO>>> GetRevenueStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null
        )
        {
            var result = await _reportService.GetRevenueStatisticsAsync(startDate, endDate);
            if (result.Status == StatusReponse.Success)
            {
                return new ResponseValue<RevenueReportDTO>(
                    result.Content,
                    StatusReponse.Success,
                    "Lấy thống kê doanh thu thành công."
                );
            }
            else if (result.Status == StatusReponse.BadRequest)
            {
                return BadRequest(new { success = false, message = result.Message });
            }
            else if (result.Status == StatusReponse.Unauthorized)
            {
                return StatusCode(403, new { success = false, message = result.Message });
            }
            return StatusCode(500, new { success = false, message = result.Message });
        }

        [HttpGet("GetDetailedRevenueReportAsync")]
        public async Task<
            ActionResult<ResponseValue<PagedResult<DetailedInvoiceDTO>>>
        > GetDetailedRevenueReportAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        )
        {
            var result = await _reportService.GetDetailedRevenueReportAsync(
                page,
                pageSize,
                search,
                startDate,
                endDate
            );
            if (result.Status == StatusReponse.Success)
            {
                return Ok(result);
            }
            if (result.Status == StatusReponse.Success)
            {
                return Ok(new { success = true, data = result.Content });
            }
            else if (result.Status == StatusReponse.BadRequest)
            {
                return BadRequest(new { success = false, message = result.Message });
            }
            else if (result.Status == StatusReponse.Unauthorized)
            {
                return StatusCode(403, new { success = false, message = result.Message });
            }
            return StatusCode(500, new { success = false, message = result.Message });
        }

        [HttpGet("GetDashBoardStaticAsync")]
        public async Task<
            ActionResult<ResponseValue<DashboardStatisticsDTO>>
        > GetDashBoardStaticAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var result = await _reportService.GetDashBoardStaticAsync(startDate, endDate);
                return new ResponseValue<DashboardStatisticsDTO>(
                    result,
                    StatusReponse.Success,
                    "Lấy thống kê thành công."
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
