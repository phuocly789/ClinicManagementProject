public class CreateScheduleRequestDTO
{
    public int? StaffId { get; set; }
    public string WorkDate { get; set; } // Format: "YYYY-MM-DD"
    public string StartTime { get; set; } // Format: "HH:mm:ss"
    public string EndTime { get; set; } // Format: "HH:mm:ss"
    public bool? IsAvailable { get; set; } = true;
}

public class UpdateScheduleRequestDTO
{
    public string WorkDate { get; set; } // Format: "YYYY-MM-DD"
    public string StartTime { get; set; } // Format: "HH:mm:ss"
    public string EndTime { get; set; } // Format: "HH:mm:ss"
    public bool? IsAvailable { get; set; } = true;
}

public class ScheduleForMedicalStaffResponse
{
    public int? StaffId { get; set; }
    public string StaffName { get; set; }
    public string Role { get; set; }
    public string WorkDate { get; set; } // Format: "YYYY-MM-DD"
    public string StartTime { get; set; } // Format: "HH:mm:ss"
    public string EndTime { get; set; } // Format: "HH:mm:ss"

    public bool? IsAvailable { get; set; } = true;
}
