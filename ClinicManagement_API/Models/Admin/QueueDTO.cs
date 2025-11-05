public class QueueDTO
{
    public int QueueId { get; set; }
    public int? PatientId { get; set; }
    public int? AppoinmentId { get; set; }
    public int? RecordId { get; set; }
    public int QueueNumber { get; set; }
    public int? RoomId { get; set; }
    public DateOnly QueueDate { get; set; }
    public TimeOnly QueueTime { get; set; }
    public string Status { get; set; } = null!;
    public int? CreatedBy { get; set; }
}

public class QueueCreateDTO
{
    public int AppointmentId { get; set; }
}

public class QueueStatusUpdateDTO
{
    public string Status { get; set; } = null!;
}