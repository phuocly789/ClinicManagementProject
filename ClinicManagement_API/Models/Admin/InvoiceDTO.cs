public class InvoiceDTO
{
    public int InvoiceId { get; set; }
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? Status { get; set; }
}