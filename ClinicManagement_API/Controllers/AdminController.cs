using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//using ClinicManagement_API.Models;

namespace ClinicManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        // [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsersAsync")]
        public async Task<IActionResult> GetAllUsersAsync(
            [FromQuery] string role = null,
            [FromQuery] string search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                _logger.LogInformation(
                    "Yêu cầu lấy danh sách người dùng với vai trò: {Role}, tìm kiếm: {Search}, trang: {Page}, kích thước trang: {PageSize}",
                    role ?? "none",
                    search ?? "none",
                    page,
                    pageSize
                );
                var result = await _adminService.GetAllUsersAsync(role, search, page, pageSize);
                return Ok(
                    new
                    {
                        success = true,
                        data = new
                        {
                            result.TotalItems,
                            result.Page,
                            result.PageSize,
                            users = result.Items,
                        },
                    }
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Dữ liệu đầu vào không hợp lệ: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Lỗi khi lấy danh sách người dùng với vai trò: {Role}, tìm kiếm: {Search}, trang: {Page}, kích thước trang: {PageSize}",
                    role ?? "none",
                    search ?? "none",
                    page,
                    pageSize
                );
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

        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive(
            int id,
            [FromBody] ToggleUserActiveRequest request
        )
        {
            try
            {
                await _adminService.ToggleUserActiveAsync(id, request.Active);
                return Ok(
                    new { success = true, message = "Cập nhật trạng thái tài khoản thành công." }
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Đã xảy ra lỗi khi cập nhật trạng thái tài khoản. Vui lòng thử lại sau.",
                    }
                );
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _adminService.DeleteUserAsync(id);
                return Ok(new { success = true, message = "Xóa người dùng thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng" });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Đã xảy ra lỗi khi xóa người dùng. Vui lòng thử lại sau.",
                    }
                );
            }
        }
    }
}
