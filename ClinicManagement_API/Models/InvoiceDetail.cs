using System;
using System.Collections.Generic;

namespace ClinicManagement_API.Models;

public partial class InvoiceDetail
{
    public int InvoiceDetailId { get; set; }

    public int? InvoiceId { get; set; }

    public int? ServiceId { get; set; }

    public int? MedicineId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal SubTotal { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual Medicine? Medicine { get; set; }

    public virtual Service? Service { get; set; }
}
