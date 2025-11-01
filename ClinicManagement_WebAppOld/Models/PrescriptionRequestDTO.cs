// PrescriptionRequestDto.cs
public class PrescriptionRequestDto
{
    public int AppointmentId { get; set; }
    public int RecordId { get; set; }
    public string Instructions { get; set; }
    public List<PrescriptionDetailRequestDto> Details { get; set; }
}

public class PrescriptionDetailRequestDto
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
    public string DosageInstruction { get; set; }
}

public class PrescriptionResponseDto
{
    public int? PrescriptionId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime? PrescriptionDate { get; set; }
    public string Instructions { get; set; }
    public List<PrescriptionDetailDataDto> Details { get; set; }
}

public class PrescriptionDetailDataDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; }
    public int Quantity { get; set; }
    public string DosageInstruction { get; set; }
}
