public class PrescriptionPrintDto
{
    public string PatientName { get; set; }
    public string DoctorName { get; set; }
    public string Symptoms { get; set; }
    public string Diagnosis { get; set; }
    public List<MedicinePrintItem> Medicines { get; set; }
}

public class MedicinePrintItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public string Usage { get; set; }
}
