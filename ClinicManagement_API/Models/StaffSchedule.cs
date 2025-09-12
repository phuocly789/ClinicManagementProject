using System;
using System.Collections.Generic;

namespace ClinicManagement_API.Models;

public partial class StaffSchedule
{
    public int ScheduleId { get; set; }

    public int? StaffId { get; set; }

    public DateOnly WorkDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User1? Staff { get; set; }
}
