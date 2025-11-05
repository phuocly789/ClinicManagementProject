using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//using ClinicManagement_API.Models;

namespace ClinicManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDiagnosisRepository _diagnosisRepository;

        public PrescriptionController(
            IPrescriptionRepository prescriptionRepository,
            IAppointmentRepository appointmentRepository,
            IDiagnosisRepository diagnosisRepository
        )
        {
            _prescriptionRepository = prescriptionRepository;
            _appointmentRepository = appointmentRepository;
            _diagnosisRepository = diagnosisRepository;
        }

        [HttpGet("GetPrescriptionDetailsAsync/{appointmentId}")]
        public async Task<ResponseValue<PrescriptionPrintDto>> GetPrescriptionDetailsAsync(
            int appointmentId
        )
        {
            try
            {
                var appointment = await _appointmentRepository
                    .GetAll()
                    .Include(a => a.Patient)
                    .Include(a => a.Staff)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                var diagnosis = await _diagnosisRepository
                    .GetAll()
                    .FirstOrDefaultAsync(d => d.AppointmentId == appointmentId);

                var prescription = await _prescriptionRepository
                    .GetAll()
                    .Where(p => p.AppointmentId == appointmentId)
                    .Include(p => p.PrescriptionDetails)
                    .ThenInclude(d => d.Medicine)
                    .FirstOrDefaultAsync();
                return new ResponseValue<PrescriptionPrintDto>(
                    new PrescriptionPrintDto
                    {
                        PatientName = appointment.Patient.FullName,
                        DoctorName = appointment.Staff.FullName,
                        Symptoms = diagnosis?.Symptoms,
                        Diagnosis = diagnosis?.Diagnosis1,
                        Medicines = prescription
                            ?.PrescriptionDetails.Select(m => new MedicinePrintItem
                            {
                                Name = m.Medicine.MedicineName,
                                Quantity = m.Quantity,
                                Usage = m.DosageInstruction,
                            })
                            .ToList(),
                    },
                    StatusReponse.Success,
                    "Lấy dữ liệu thành công"
                );
            }
            catch (Exception ex)
            {
                return new ResponseValue<PrescriptionPrintDto>(
                    null,
                    StatusReponse.Error,
                    ex.Message
                );
            }
        }
    }
}
