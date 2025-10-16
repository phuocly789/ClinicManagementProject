using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Data.Models;

public partial class Diagnosis
{
    public int DiagnosisId { get; set; }

    public int? AppointmentId { get; set; }

    public int? StaffId { get; set; }

    public int? RecordId { get; set; }

    public string? Symptoms { get; set; }

    public string? Diagnosis1 { get; set; }

    public string? Notes { get; set; }

    public DateTime? DiagnosisDate { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual MedicalRecord? Record { get; set; }

    public virtual User? Staff { get; set; }
}
