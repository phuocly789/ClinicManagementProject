using ClinicManagement_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

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
    Task<ResponseValue<PagedResult<DetailedInvoiceDTO>>> GetDetailedRevenueReportAsync(
        int page = 1,
        int pageSize = 10,
        string search = null,
        DateTime? startDate = null,
        DateTime? endDate = null
    );
    Task<DashboardStatisticsDTO> GetDashBoardStaticAsync(
        DateTime? startDate = null,
        DateTime? endDate = null
    );
}

public class ReportsService : IReportsService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<ReportsService> _logger;
    private readonly SupabaseContext _context;

    public ReportsService(
        IAppointmentRepository appointmentRepository,
        IInvoiceRepository invoiceRepository,
        ILogger<ReportsService> logger,
        SupabaseContext context
    )
    {
        _appointmentRepository = appointmentRepository;
        _invoiceRepository = invoiceRepository;
        _logger = logger;
        _context = context;
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

    public async Task<ResponseValue<PagedResult<DetailedInvoiceDTO>>> GetDetailedRevenueReportAsync(
        int page = 1,
        int pageSize = 10,
        string search = null,
        DateTime? startDate = null,
        DateTime? endDate = null
    )
    {
        try
        {
            // Validate dates
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return new ResponseValue<PagedResult<DetailedInvoiceDTO>>(
                    null,
                    StatusReponse.BadRequest,
                    "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc."
                );
            }

            // Default date range
            var fromDate = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = endDate ?? DateTime.UtcNow.AddDays(1).AddTicks(-1); // Bao gồm cuối ngày

            int? searchId = null;
            if (int.TryParse(search, out int parsedId))
            {
                searchId = parsedId;
            }

            // Truy vấn LINQ
            var query =
                from i in _context.Invoices
                join id in _context.InvoiceDetails on i.InvoiceId equals id.InvoiceId
                join u in _context.Users on i.PatientId equals u.UserId into users
                from u in users.DefaultIfEmpty()
                join a in _context.Appointments
                    on i.AppointmentId equals a.AppointmentId
                    into appointments
                from a in appointments.DefaultIfEmpty()
                join s in _context.Services on id.ServiceId equals s.ServiceId into services
                from s in services.DefaultIfEmpty()
                join m in _context.Medicines on id.MedicineId equals m.MedicineId into medicines
                from m in medicines.DefaultIfEmpty()
                where
                    i.InvoiceDate >= fromDate
                    && i.InvoiceDate <= toDate
                    && (
                        string.IsNullOrEmpty(search)
                        || u.FullName.Contains(search)
                        || (searchId.HasValue && i.InvoiceId == searchId.Value)
                    )
                select new RawInvoiceDetailDTO
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceDate = i.InvoiceDate,
                    TotalAmount = i.TotalAmount,
                    PatientName = u != null ? u.FullName : null,
                    AppointmentDate = a != null ? (DateOnly)a.AppointmentDate : null,

                    ServiceId = id.ServiceId,
                    ServiceName = s != null ? s.ServiceName : null,
                    MedicineId = id.MedicineId,
                    MedicineName = m != null ? m.MedicineName : null,
                    Quantity = id.Quantity,
                    UnitPrice = id.UnitPrice,
                    SubTotal = id.SubTotal,
                    Status = id.Invoice.Status,
                };

            // Nhóm dữ liệu theo InvoiceId và áp dụng phân trang
            var groupedInvoices = await query
                .GroupBy(r => r.InvoiceId)
                .Select(g => new DetailedInvoiceDTO
                {
                    InvoiceId = g.Key,
                    InvoiceDate = g.First().InvoiceDate,
                    TotalAmount = g.First().TotalAmount,
                    PatientName = g.First().PatientName,
                    AppointmentDate = g.First().AppointmentDate,
                    Status = g.First().Status,
                    Details = g.Select(item => new DetailedInvoiceItemDTO
                        {
                            ServiceId = item.ServiceId,
                            ServiceName = item.ServiceName,
                            MedicineId = item.MedicineId,
                            MedicineName = item.MedicineName,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            SubTotal = item.SubTotal,
                        })
                        .ToList(),
                })
                .OrderByDescending(i => i.InvoiceDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tính tổng số items
            var totalItemsQuery =
                from i in _context.Invoices
                join id in _context.InvoiceDetails on i.InvoiceId equals id.InvoiceId
                join u in _context.Users on i.PatientId equals u.UserId into users
                from u in users.DefaultIfEmpty()
                where
                    i.Status == "Paid"
                    && i.InvoiceDate >= fromDate
                    && i.InvoiceDate <= toDate
                    && (
                        string.IsNullOrEmpty(search)
                        || u.FullName.Contains(search)
                        || (searchId.HasValue && i.InvoiceId == searchId.Value)
                    )
                select i.InvoiceId;

            var totalItems = await totalItemsQuery.Distinct().CountAsync();

            var pagedResult = new PagedResult<DetailedInvoiceDTO>
            {
                Items = groupedInvoices,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
            };

            return new ResponseValue<PagedResult<DetailedInvoiceDTO>>(
                pagedResult,
                StatusReponse.Success,
                "Lấy báo cáo doanh thu chi tiết thành công."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy báo cáo doanh thu chi tiết");
            return new ResponseValue<PagedResult<DetailedInvoiceDTO>>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy báo cáo doanh thu chi tiết: " + ex.Message
            );
        }
    }

    public async Task<DashboardStatisticsDTO> GetDashBoardStaticAsync(
        DateTime? startDate = null,
        DateTime? endDate = null
    )
    {
        // Nếu không truyền vào thì mặc định lấy 1 tháng gần nhất
        var fromDate = startDate ?? DateTime.UtcNow.AddDays(-6);
        var toDate = endDate ?? DateTime.UtcNow;

        // Chuyển về DateOnly để so sánh đúng kiểu trong DB
        var fromDateOnly = DateOnly.FromDateTime(fromDate.Date);
        var toDateOnly = DateOnly.FromDateTime(toDate.Date);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Tổng số lịch hẹn hôm nay
        var totalAppointments = await _appointmentRepository
            .GetAll()
            .CountAsync(a => a.AppointmentDate == today);

        // Số lịch hẹn đã khám trong khoảng thời gian
        var completedAppointments = await _appointmentRepository
            .GetAll()
            .CountAsync(a =>
                a.AppointmentDate >= fromDateOnly
                && a.AppointmentDate <= toDateOnly
                && a.Status == "Đã khám"
            );

        // Số hóa đơn đang pending trong khoảng
        var pendingInvoicesCount = await _invoiceRepository
            .GetAll()
            .CountAsync(i => i.InvoiceDate.HasValue && i.Status == "Pending");

        // Kết quả trả về
        var statistics = new DashboardStatisticsDTO
        {
            TotalAppointmentsToday = totalAppointments,
            CompletedAppointmentsToday = completedAppointments,
            PendingInvoicesCount = pendingInvoicesCount,
        };

        return statistics;
    }
}

public class RawInvoiceDetailDTO
{
    public int InvoiceId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PatientName { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int? MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public string Status { get; set; }
}
