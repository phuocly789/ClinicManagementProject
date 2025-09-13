using System;
using System.Collections.Generic;

namespace ClinicManagement_API.Models;

public partial class PatientHistory
{
    public int? PatientId { get; set; }

    public string? FullName { get; set; }

    public long? TotalVisits { get; set; }

    public long? TotalRecords { get; set; }

    public DateOnly? LastVisitDate { get; set; }
}
