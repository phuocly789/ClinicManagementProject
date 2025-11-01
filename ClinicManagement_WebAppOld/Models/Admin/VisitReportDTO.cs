public class VisitReportDTO
{
    public int TotalVisits { get; set; }
    public List<VisitByDateDTO> ByDate { get; set; } = new List<VisitByDateDTO>();
}

public class VisitByDateDTO
{
    public string Date { get; set; } = string.Empty;
    public int VisitCount { get; set; }
}

// DTOs for Revenue Statistics
public class RevenueReportDTO
{
    public decimal TotalRevenue { get; set; }
    public List<RevenueByDateDTO> ByDate { get; set; } = new List<RevenueByDateDTO>();
}

public class RevenueByDateDTO
{
    public string Date { get; set; }
    public decimal Revenue { get; set; }
}
