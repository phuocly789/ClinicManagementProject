using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("GetMySchedule")]
        public async Task<ResponseValue<List<AppointmentMyScheduleDto>>> GetMySchedule(
            int staffId,
            [FromQuery] DateOnly? date = null
        )
        {
            try
            {
                var appointments = await _appointmentService.GetAppointmentsByStaffIdAnddDateAsync(
                    staffId,
                    date
                );
                return new ResponseValue<List<AppointmentMyScheduleDto>>(
                    appointments,
                    StatusReponse.Success,
                    "Lấy dữ liệu thành công"
                );
            }
            catch (Exception ex)
            {
                return new ResponseValue<List<AppointmentMyScheduleDto>>(
                    null,
                    StatusReponse.Error,
                    ex.Message
                );
            }
        }
    }
}
