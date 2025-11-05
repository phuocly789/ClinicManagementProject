public class DoctorQueueDto
{
    public int QueueId { get; set; }
    public int? AppoinmentId { get; set; }
    public int QueueNumber { get; set; }
    public string PatientName { get; set; }
    public TimeOnly QueueTime { get; set; }
    public string Status { get; set; }
}
