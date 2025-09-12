using System;
using System.Collections.Generic;

namespace ClinicManagement_API.Models;

public partial class MedicalStaff
{
    public int StaffId { get; set; }

    public string StaffType { get; set; } = null!;

    public string? Specialty { get; set; }

    public string? LicenseNumber { get; set; }

    public string? Bio { get; set; }

    public virtual User1 Staff { get; set; } = null!;
}
