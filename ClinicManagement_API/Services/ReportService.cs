using Microsoft.EntityFrameworkCore;

public interface IReportsService
{
    Task<ResponseValue<VisitReportDTO>> GetVisitStatisticsAsync(
        DateOnly? startDate = null,
        DateOnly? endDate = null
    );
    Task<ResponseValue<RevenueReportDTO>> GetRevenueStatisticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null
    );
}

public class ReportsService : IReportsService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<ReportsService> _logger;

    public ReportsService(
        IAppointmentRepository appointmentRepository,
        IInvoiceRepository invoiceRepository,
        ILogger<ReportsService> logger
    )
    {
        _appointmentRepository = appointmentRepository;
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    public async Task<ResponseValue<VisitReportDTO>> GetVisitStatisticsAsync(
        DateOnly? startDate = null,
        DateOnly? endDate = null
    )
    {
        try
        {
            //validate date
            if (startDate != null && endDate != null && startDate > endDate)
            {
                return new ResponseValue<VisitReportDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Ngày bắt đầu phải nhỏ hơn ngày kết thúc."
                );
            }
            //defaut date range if not provided
            var fromDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)); //1 tháng trước
            var toDate = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            var query = _appointmentRepository
                .GetAll()
                .AsNoTracking()
                .Where(a =>
                    a.Status == "Đã khám"
                    && a.AppointmentDate >= fromDate
                    && a.AppointmentDate <= toDate
                );

            //get to visit
            var totalItems = await query.CountAsync();

            var visitsByDate = query
                .GroupBy(a => a.AppointmentDate)
                .AsEnumerable() // chuyển sang LINQ to Objects
                .Select(g => new VisitByDateDTO
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    VisitCount = g.Count(),
                })
                .OrderBy(g => g.Date)
                .ToList(); // dùng ToList thay vì ToListAsync

            return new ResponseValue<VisitReportDTO>(
                new VisitReportDTO { TotalVisits = totalItems, ByDate = visitsByDate },
                StatusReponse.Success,
                "Lấy thống kê lượt khám thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<VisitReportDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy thống kê : " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<RevenueReportDTO>> GetRevenueStatisticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null
    )
    {
        try
        {
            // Validate dates
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return new ResponseValue<RevenueReportDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc."
                );
            }

            //defaut date range if not provided
            var fromDate = startDate ?? DateTime.UtcNow.AddMonths(-1); //1 tháng trước
            var toDate = endDate ?? DateTime.UtcNow;

            var query = _invoiceRepository
                .GetAll()
                .AsNoTracking()
                .Where(i =>
                    i.Status == "Paid" && i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate
                );

            //get total
            var totalRevenue = await query.SumAsync(i => i.TotalAmount);

            // Get revenue by date
            var revenueByDateRaw = await query
                .GroupBy(i => i.InvoiceDate.HasValue ? i.InvoiceDate.Value.Date : DateTime.MinValue)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
                .ToListAsync();

            var revenueByDate = revenueByDateRaw
                .Select(g => new RevenueByDateDTO
                {
                    Date =
                        g.Date == DateTime.MinValue ? string.Empty : g.Date.ToString("yyyy-MM-dd"),
                    Revenue = g.Revenue,
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new ResponseValue<RevenueReportDTO>(
                new RevenueReportDTO { TotalRevenue = totalRevenue, ByDate = revenueByDate },
                StatusReponse.Success,
                "Lấy thống kê doanh thu thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<RevenueReportDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy thống kê doanh thu: " + ex.Message
            );
        }
    }
}
