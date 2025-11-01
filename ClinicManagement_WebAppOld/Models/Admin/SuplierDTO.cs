//   "supplierId": 1,
//         "supplierName": "Công ty Dược ABC",
//         "contactEmail": "abc@pharma.com",
//         "contactPhone": "0901234567",
//         "address": "123 Đường DEF, TP.HCM",
//         "description": "Nhà cung cấp thuốc uy tín"

public class SupplierDTO
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public string Address { get; set; }
    public string? Description { get; set; }
}
