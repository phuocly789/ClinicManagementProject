using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Data.Models;

public partial class MedicalRecordDetail
{
    public int? RecordId { get; set; }

    public string? RecordNumber { get; set; }

    public DateOnly? IssuedDate { get; set; }

    public int? PatientId { get; set; }

    public string? PatientName { get; set; }

    public string? RecordStatus { get; set; }

    public string? Appointments { get; set; }
}
