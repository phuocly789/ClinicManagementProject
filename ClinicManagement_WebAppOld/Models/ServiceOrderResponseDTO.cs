public class CreateServiceOrderDto
{
    public int AppointmentId { get; set; }
    public int ServiceId { get; set; }
    public int AssignedStaffId { get; set; }
}

public class ServiceOrderResponseDto
{
    public int ServiceOrderId { get; set; }
    public int? AppointmentId { get; set; }
    public int? ServiceId { get; set; }
    public int? AssignedStaffId { get; set; }
    public DateTime? OrderDate { get; set; }
    public string Status { get; set; }
}

public class ServiceOrderResultDto
{
    public int ServiceOrderId { get; set; }
    public int? ServiceId { get; set; }
    public string ServiceName { get; set; }
    public int? AssignedStaffId { get; set; }
    public string Result { get; set; }
    public string Status { get; set; }
    public DateTime? OrderDate { get; set; }
}
