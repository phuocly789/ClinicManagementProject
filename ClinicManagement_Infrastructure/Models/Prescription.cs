using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Models;

public partial class Prescription
{
    public int PrescriptionId { get; set; }

    public int? AppointmentId { get; set; }

    public int? StaffId { get; set; }

    public int? RecordId { get; set; }

    public DateTime? PrescriptionDate { get; set; }

    public string? Instructions { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();

    public virtual MedicalRecord? Record { get; set; }

    public virtual User? Staff { get; set; }
}
