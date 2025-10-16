public class LowStockMedicineDTO
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; }
    public int StockQuantity { get; set; }
    public string Unit { get; set; }
}

public class RevenueByDayDTO
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
}


public class TodaysAppointmentDTO
{
    public int AppointmentId { get; set; }
    public TimeOnly AppointmentTime { get; set; } // Sử dụng TimeOnly cho .NET 6+
    public string PatientName { get; set; }
    public string DoctorName { get; set; }
    public string Status { get; set; }
}
public class DashboardStatisticsDTO
{
    public int TotalAppointmentsToday { get; set; }
    public int CompletedAppointmentsToday { get; set; }
    public int PendingInvoicesCount { get; set; }
}
