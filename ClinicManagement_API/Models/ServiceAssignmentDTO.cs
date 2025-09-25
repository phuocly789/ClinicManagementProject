// ServiceAssignmentDto.cs
public class ServiceAssignmentDto
{
    public int ServiceOrderId { get; set; }
    public int? AppointmentId { get; set; }
    public int? PatientId { get; set; }
    public string PatientName { get; set; }
    public int? ServiceId { get; set; }
    public string ServiceName { get; set; }
    public DateTime? OrderDate { get; set; }
    public string Status { get; set; }
}

public class ServiceAssignmentDTO
{
    public List<ServiceAssignmentDto> ServiceAssignments { get; set; }
}
public class ServiceOrderUpdateDto
{
    public string Result { get; set; }
    public string Status { get; set; } // Chỉ cho phép 'Completed' hoặc 'Cancelled'
}