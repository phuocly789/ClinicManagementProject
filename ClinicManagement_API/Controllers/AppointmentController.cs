using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IServiceBase<Appointment> _appointmentRepository;

        public AppointmentController(IServiceBase<Appointment> appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        [HttpGet("GetAllAppointments")]
        public async Task<ActionResult> GetAllAppointments()
        {
            var appointments = await _appointmentRepository.GetAllAsync();
            return Ok(appointments);
        }
    }
}
