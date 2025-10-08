public class ImportReportDTO
{
    public int TotalBills { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalQuantity { get; set; }
    public Dictionary<string, int> SupplierCounts { get; set; }
    public PagedResult<ImportReportItemDTO> Details { get; set; }
}

public class ImportReportItemDTO
{
    public int ImportId { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; }
    public DateTime ImportDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; }
    public int? CreatedBy { get; set; }
    public List<ImportDetailItemDTO> Details { get; set; }
}


