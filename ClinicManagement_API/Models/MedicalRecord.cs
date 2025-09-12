using System;
using System.Collections.Generic;

namespace ClinicManagement_API.Models;

public partial class MedicalRecord
{
    public int RecordId { get; set; }

    public int? PatientId { get; set; }

    public string RecordNumber { get; set; } = null!;

    public DateOnly IssuedDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User1? CreatedByNavigation { get; set; }

    public virtual ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();

    public virtual Patient? Patient { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<Queue> Queues { get; set; } = new List<Queue>();
}
