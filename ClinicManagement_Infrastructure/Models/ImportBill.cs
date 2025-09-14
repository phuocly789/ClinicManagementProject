using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Models;

public partial class ImportBill
{
    public int ImportId { get; set; }

    public int? SupplierId { get; set; }

    public DateTime? ImportDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<ImportDetail> ImportDetails { get; set; } = new List<ImportDetail>();

    public virtual Supplier? Supplier { get; set; }
}
