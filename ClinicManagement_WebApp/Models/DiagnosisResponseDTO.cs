public class CreateDiagnosisDto
{
    public int AppointmentId { get; set; }
    public int RecordId { get; set; }
    public string Symptoms { get; set; }
    public string Diagnosis { get; set; }
    public string Notes { get; set; }
}


public class DiagnosisDataDto
{
    public int DiagnosisId { get; set; }
    public int? AppointmentId { get; set; }
    public int? StaffId { get; set; }
    public string? Symptoms { get; set; }
    public string? Diagnosis { get; set; }
    public DateTime? DiagnosisDate { get; set; }
}
