using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public partial class Queue
{
    public int QueueId { get; set; }

    public int? PatientId { get; set; }

    public int? AppointmentId { get; set; }

    public int? RecordId { get; set; }

    public int QueueNumber { get; set; }

    public int? RoomId { get; set; }

    public DateOnly QueueDate { get; set; }

    public TimeOnly QueueTime { get; set; }

    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual User1? CreatedByNavigation { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual MedicalRecord? Record { get; set; }

    public virtual Room? Room { get; set; }
}
