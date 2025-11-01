public class ServiceDTO
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
}
