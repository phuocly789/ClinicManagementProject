public class DetailedInvoiceDTO
{
    public int InvoiceId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string PatientName { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public List<DetailedInvoiceItemDTO> Details { get; set; } = new();
}

public class DetailedInvoiceItemDTO
{
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int? MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}
