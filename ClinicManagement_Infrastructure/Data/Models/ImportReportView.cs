using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public partial class ImportReportView
{
    public int? ImportId { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public DateTime? ImportDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }

    public int? ImportDetailId { get; set; }

    public int? MedicineId { get; set; }

    public string? MedicineName { get; set; }

    public int? Quantity { get; set; }

    public decimal? ImportPrice { get; set; }

    public decimal? SubTotal { get; set; }
}
