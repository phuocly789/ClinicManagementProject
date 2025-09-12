using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int? AppointmentId { get; set; }

    public int? PatientId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Appointment? Appointment { get; set; }

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual User1? Patient { get; set; }
}
