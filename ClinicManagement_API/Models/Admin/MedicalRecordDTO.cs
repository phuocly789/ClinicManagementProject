public partial class MedicalRecordDTO
{
    public int RecordId { get; set; }
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string RecordNumber { get; set; } = null!;
    public DateOnly IssuedDate { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }

}
