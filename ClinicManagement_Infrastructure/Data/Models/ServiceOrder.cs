using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public partial class ServiceOrder
{
    public int ServiceOrderId { get; set; }

    public int? AppointmentId { get; set; }

    public int? ServiceId { get; set; }

    public int? AssignedStaffId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Result { get; set; }

    public string Status { get; set; } = null!;

    public virtual Appointment? Appointment { get; set; }

    public virtual User1? AssignedStaff { get; set; }

    public virtual Service? Service { get; set; }
}
