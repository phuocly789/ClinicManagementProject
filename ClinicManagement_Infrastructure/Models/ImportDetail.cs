using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Models;

public partial class ImportDetail
{
    public int ImportDetailId { get; set; }

    public int? ImportId { get; set; }

    public int? MedicineId { get; set; }

    public int Quantity { get; set; }

    public decimal ImportPrice { get; set; }

    public decimal SubTotal { get; set; }

    public virtual ImportBill? Import { get; set; }

    public virtual Medicine? Medicine { get; set; }
}
