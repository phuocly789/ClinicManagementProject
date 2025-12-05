using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace ClinicManagement_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        
        [HttpGet("GetAllRooms")]
        public async Task<IActionResult> GetAllRoomsAsync()
        {
            try
            {
                var result = await _roomService.GetAllRoomsAsync();

                if (result == null)
                    throw new Exception("Service trả về null");

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Có lỗi xảy ra khi lấy danh sách phòng.",
                        error = ex.Message,
                    }
                );
            }
        }
    }
}
