
public class AppointmentDTO
{
    public int AppointmentId { get; set; }
    public int? PatientId { get; set; }
    public int? StaffId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class AppointmentCreateDTO
{
    public int PatientId { get; set; }
    public int StaffId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public string? Notes { get; set; }
}

public class AppointmentStatusDTO
{
    public string? Status { get; set; }
}