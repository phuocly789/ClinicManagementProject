using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Data.Models;

public partial class Supplier
{
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<ImportBill> ImportBills { get; set; } = new List<ImportBill>();
}
