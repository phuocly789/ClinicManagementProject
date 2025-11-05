using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Data.Models;

public partial class StaffSchedule
{
    public int ScheduleId { get; set; }

    public int? StaffId { get; set; }

    public DateOnly WorkDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool? IsAvailable { get; set; }

    public int? RoomId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Room? Room { get; set; }

    public virtual User? Staff { get; set; }
}
