using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

//using ClinicManagement_API.Models;

namespace ClinicManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechnicianController : ControllerBase
    {
        // private readonly ILogger<TechnicianController> _logger;
        private readonly ITechnicianService _technicianService;

        public TechnicianController(ITechnicianService technicianService)
        {
            _technicianService = technicianService;
        }

        [HttpGet("GetAssignedServiceOrders")]
        public async Task<
            ActionResult<ResponseValue<ServiceAssignmentDTO>>
        > GetAssignedServiceOrders([FromQuery] DateTime? date)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Unauthorized(
                    new { Success = false, Message = "Không thể xác thực người dùng" }
                );
            }

            var result = await _technicianService.GetAssignedServiceOrdersAsync(
                date,
                currentUserId
            );
            return Ok(result);
        }

        [HttpPut("UpdateServiceOrderResult/{serviceOrderId}")]
        public async Task<
            ActionResult<ResponseValue<ServiceOrderResultDto>>
        > UpdateServiceOrderResult(
            int serviceOrderId,
            [FromBody] ServiceOrderUpdateDto updateDto,
            int currentUserId
        )
        {
            // var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (!int.TryParse(currentUserIdClaim, out var currentUserId))
            // {
            //     return Unauthorized(
            //         new { Success = false, Message = "Không thể xác thực người dùng" }
            //     );
            // }

            var result = await _technicianService.UpdateServiceOrderResultAsync(
                serviceOrderId,
                updateDto,
                currentUserId
            );
            if (result.Status == StatusReponse.BadRequest)
            {
                return BadRequest(result);
            }
            if (result.Status == StatusReponse.NotFound)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
