using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public partial class Medicine
{
    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = null!;

    public string? MedicineType { get; set; }

    public string Unit { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<ImportDetail> ImportDetails { get; set; } = new List<ImportDetail>();

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
