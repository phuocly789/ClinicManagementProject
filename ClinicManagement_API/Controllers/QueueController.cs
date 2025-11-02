using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [Authorize(Roles = "Receptionist,Doctor")]
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueService _queueService;

        public QueueController(IQueueService queueService)
        {
            _queueService = queueService;
        }

        [HttpGet("queues/room/{roomId}")]
        public async Task<IActionResult> GetQueues(int roomId, [FromQuery] DateOnly date)
        {
            try
            {
                var result = await _queueService.GetQueuesAsync(roomId, date);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Có lỗi xảy ra khi lấy hàng chờ.",
                        error = ex.Message,
                    }
                );
            }
        }

        [HttpPost("CreateQueueAsync")]
        public async Task<ActionResult<ResponseValue<QueueDTO>>> CreateQueueAsync(
            [FromBody] QueueCreateDTO request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _queueService.AddToQueueAsync(request);

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

        [HttpPut("UpdateStatusQueueAsync/{id}")]
        public async Task<ActionResult<ResponseValue<SupplierDTO>>> UpdateStatusQueueAsync(
            int id,
            [FromBody] QueueStatusUpdateDTO request
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _queueService.QueueUpdateStatusAsync(id, request);

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

        [HttpPut("start/{queueId}")]
        public async Task<IActionResult> StartExamination(int queueId)
        {
            var result = await _queueService.QueueUpdateStatusAsync(
                queueId,
                new QueueStatusUpdateDTO { Status = "InProgress" }
            );
            if (result.Status == StatusReponse.Success)
            {
                return Ok(new { success = true, message = "Bắt đầu khám." });
            }
            return BadRequest(new { success = false, message = result.Message });
        }
    }
}
