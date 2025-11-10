using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Data.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string? MedicalHistory { get; set; }

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } =
        new List<MedicalRecord>();

    public virtual User PatientNavigation { get; set; } = null!;

    public virtual ICollection<Queue> Queues { get; set; } = new List<Queue>();
}
