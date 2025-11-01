public class ImportDTO
{
    public int ImportId { get; set; }
    public int? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime? ImportDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
}

public class ImportCreateDTO
{
    public int SupplierId { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public List<ImportDetailDTO> Details { get; set; } = new List<ImportDetailDTO>();
}

public class ImportDetailDTO
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
    public decimal ImportPrice { get; set; }
}

public class ImportDetailByIdDTO
{
    public int ImportId { get; set; }
    public int? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime? ImportDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<ImportDetailItemDTO> Details { get; set; } = new List<ImportDetailItemDTO>();
}

public class ImportDetailItemDTO
{
    public int? MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ImportPrice { get; set; }
    public decimal SubTotal { get; set; }
}
