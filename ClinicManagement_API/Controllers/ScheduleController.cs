using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

//using ClinicManagement_API.Models;

namespace ClinicManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            IScheduleService scheduleService,
            ILogger<ScheduleController> logger
        )
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        [HttpGet("GetAllSchedulesAsync")]
        public async Task<
            ActionResult<ResponseValue<PagedResult<ScheduleForMedicalStaffResponse>>>
        > GetAllSchedulesAsync()
        {
            try
            {
                var result = await _scheduleService.GetAllSchedulesAsync();
                return result.Status switch
                {
                    StatusReponse.Success => Ok(result),
                    StatusReponse.BadRequest => BadRequest(result),
                    StatusReponse.Unauthorized => StatusCode(403, result),
                    StatusReponse.NotFound => NotFound(result),
                    _ => StatusCode(500, result),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllSchedulesAsync");
                return StatusCode(
                    500,
                    new ResponseValue<PagedResult<ScheduleForMedicalStaffResponse>>(
                        null,
                        StatusReponse.Error,
                        "An error occurred while processing your request."
                    )
                );
            }
        }

        [HttpPost("CreateScheduleAsync")]
        public async Task<
            ActionResult<ResponseValue<CreateScheduleRequestDTO>>
        > CreateScheduleAsync([FromBody] CreateScheduleRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        new ResponseValue<CreateScheduleRequestDTO>(
                            null,
                            StatusReponse.BadRequest,
                            "Invalid input data."
                        )
                    );
                }
                var result = await _scheduleService.CreateScheduleAsync(request);
                if (result.Status == StatusReponse.Success)
                {
                    return Ok(result);
                }
                else if (result.Status == StatusReponse.BadRequest)
                {
                    return BadRequest(result);
                }
                else if (result.Status == StatusReponse.Unauthorized)
                {
                    return StatusCode(403, result);
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateScheduleAsync");
                return StatusCode(
                    500,
                    new ResponseValue<CreateScheduleRequestDTO>(
                        null,
                        StatusReponse.Error,
                        "An error occurred while processing your request."
                    )
                );
            }
        }

        [HttpPut("UpdateScheduleAsync/{scheduleId}")]
        public async Task<
            ActionResult<ResponseValue<UpdateScheduleRequestDTO>>
        > UpdateScheduleAsync(int scheduleId, [FromBody] UpdateScheduleRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        new ResponseValue<UpdateScheduleRequestDTO>(
                            null,
                            StatusReponse.BadRequest,
                            "Invalid input data."
                        )
                    );
                }
                var result = await _scheduleService.UpdateScheduleAsync(scheduleId, request);
                if (result.Status == StatusReponse.Success)
                {
                    return Ok(result);
                }
                else if (result.Status == StatusReponse.BadRequest)
                {
                    return BadRequest(result);
                }
                else if (result.Status == StatusReponse.Unauthorized)
                {
                    return StatusCode(403, result);
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateScheduleAsync");
                return StatusCode(
                    500,
                    new ResponseValue<CreateScheduleRequestDTO>(
                        null,
                        StatusReponse.Error,
                        "An error occurred while processing your request."
                    )
                );
            }
        }

        [HttpDelete("DeleteScheduleAsync/{scheduleId}")]
        public async Task<ActionResult<ResponseValue<bool>>> DeleteScheduleAsync(int scheduleId)
        {
            try
            {
                var result = await _scheduleService.DeleteScheduleAsync(scheduleId);

                return result.Status switch
                {
                    StatusReponse.Success => Ok(result),
                    StatusReponse.BadRequest => BadRequest(result),
                    StatusReponse.Unauthorized => StatusCode(403, result),
                    StatusReponse.NotFound => NotFound(result),
                    _ => StatusCode(500, result),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteScheduleAsync");
                return StatusCode(
                    500,
                    new ResponseValue<bool>(
                        false,
                        StatusReponse.Error,
                        "An error occurred while processing your request."
                    )
                );
            }
        }
    }
}
