// {
//     "serviceId": 1,
//     "serviceName": "Khám nội tổng quát",
//     "serviceType": "Examination",
//     "price": 200000,
//     "description": "Khám và tư vấn các bệnh lý nội khoa."
//   },
public class ServiceVM
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty; // Ví dụ: "Examination", "Test", "Procedure"
    public decimal Price { get; set; }
    public string? Description { get; set; }
}
