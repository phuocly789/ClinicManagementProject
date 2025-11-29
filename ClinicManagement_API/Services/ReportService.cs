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
    Task<ResponseValue<CombinedRevenueReportDTO>> GetCombinedRevenueReportAsync(
        DateTime? startDate = null,
        DateTime? endDate = null
    );
    Task<ResponseValue<PrescriptionAnalyticsDTO>> GetPrescriptionAnalyticsAsync();
    Task<ResponseValue<RevenueForecastDTO>> GetRevenueForecastAsync();
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
                    i.InvoiceDate >= fromDate
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

    // Thêm vào ReportsService
    public async Task<ResponseValue<CombinedRevenueReportDTO>> GetCombinedRevenueReportAsync(
        DateTime? startDate = null,
        DateTime? endDate = null
    )
    {
        try
        {
            // Validate dates
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return new ResponseValue<CombinedRevenueReportDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc."
                );
            }

            // Default date range
            var fromDate = startDate ?? DateTime.UtcNow.AddDays(-6);
            var toDate = endDate ?? DateTime.UtcNow;

            // Chuyển về DateOnly để so sánh appointment
            var fromDateOnly = DateOnly.FromDateTime(fromDate.Date);
            var toDateOnly = DateOnly.FromDateTime(toDate.Date);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Lấy dữ liệu doanh thu
            var revenueQuery = _invoiceRepository
                .GetAll()
                .AsNoTracking()
                .Where(i =>
                    i.Status == "Paid" && i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate
                );

            var totalRevenue = await revenueQuery.SumAsync(i => i.TotalAmount);

            var revenueByDateRaw = await revenueQuery
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

            // Lấy thống kê appointments
            var totalAppointmentsToday = await _appointmentRepository
                .GetAll()
                .CountAsync(a => a.AppointmentDate == today);

            var completedAppointmentsToday = await _appointmentRepository
                .GetAll()
                .CountAsync(a =>
                    a.AppointmentDate >= fromDateOnly
                    && a.AppointmentDate <= toDateOnly
                    && a.Status == "Completed"
                );

            var pendingInvoicesCount = await _invoiceRepository
                .GetAll()
                .CountAsync(i => i.InvoiceDate.HasValue && i.Status == "Pending");

            var combinedReport = new CombinedRevenueReportDTO
            {
                TotalRevenue = totalRevenue,
                RevenueByDate = revenueByDate,
                TotalAppointmentsToday = totalAppointmentsToday,
                CompletedAppointmentsToday = completedAppointmentsToday,
                PendingInvoicesCount = pendingInvoicesCount,
            };

            return new ResponseValue<CombinedRevenueReportDTO>(
                combinedReport,
                StatusReponse.Success,
                "Lấy báo cáo tổng hợp thành công."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy báo cáo tổng hợp");
            return new ResponseValue<CombinedRevenueReportDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy báo cáo tổng hợp: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<RevenueForecastDTO>> GetRevenueForecastAsync()
    {
        try
        {
            // Lấy 6 tháng gần nhất
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-6);

            var historicalData = await _context
                .Invoices.Where(i =>
                    i.Status == "Paid" && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate
                )
                .GroupBy(i => new { i.InvoiceDate.Value.Year, i.InvoiceDate.Value.Month })
                .Select(g => new RevenueHistoryDTO
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(i => i.TotalAmount),
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            if (!historicalData.Any())
            {
                return new ResponseValue<RevenueForecastDTO>(
                    null,
                    StatusReponse.Error,
                    "Không có đủ dữ liệu để dự báo."
                );
            }

            // ----------------------------
            //  Tính Hồi Quy Tuyến Tính Y = aX + b
            // ----------------------------

            int n = historicalData.Count;

            // Biến x = 1,2,3... theo thứ tự tháng
            var xValues = Enumerable.Range(1, n).Select(x => (decimal)x).ToList();
            var yValues = historicalData.Select(h => (decimal)h.Revenue).ToList();

            decimal sumX = xValues.Sum();
            decimal sumY = yValues.Sum();
            decimal sumXY = xValues.Zip(yValues, (x, y) => x * y).Sum();
            decimal sumX2 = xValues.Select(x => x * x).Sum();

            // Hệ số hồi quy
            decimal a = (n * sumXY - sumX * sumY) / (n * sumX2 - (sumX * sumX));
            decimal b = (sumY - a * sumX) / n;

            // Dự báo tháng kế tiếp
            decimal xNext = n + 1;
            decimal predictedRevenue = a * xNext + b;

            // Confidence: đơn giản hóa
            decimal confidence = n >= 3 ? 0.85m : 0.60m;

            var forecast = new RevenueForecastDTO
            {
                Historical = historicalData
                    .Select(h => new RevenueHistoryItemDTO
                    {
                        Month = h.Month,
                        Year = h.Year,
                        Revenue = h.Revenue,
                    })
                    .ToList(),

                Forecast = new RevenuePredictionDTO
                {
                    PredictedRevenue = predictedRevenue,
                    Confidence = confidence,
                    NextMonth = DateTime.UtcNow.AddMonths(1).Month,
                    NextYear = DateTime.UtcNow.AddMonths(1).Year,
                },
            };

            return new ResponseValue<RevenueForecastDTO>(
                forecast,
                StatusReponse.Success,
                "Dự báo doanh thu bằng hồi quy tuyến tính thành công."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tính dự báo doanh thu");
            return new ResponseValue<RevenueForecastDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi dự báo doanh thu: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<PrescriptionAnalyticsDTO>> GetPrescriptionAnalyticsAsync()
    {
        try
        {
            // Lấy dữ liệu 3 tháng gần nhất
            var startDate = DateTime.UtcNow.AddMonths(-3);

            // Top thuốc được kê nhiều nhất
            var topMedicines = await _context
                .PrescriptionDetails.Where(pd => pd.Prescription.PrescriptionDate >= startDate)
                .GroupBy(pd => new { pd.MedicineId, pd.Medicine.MedicineName })
                .Select(g => new TopMedicineDTO
                {
                    MedicineId = g.Key.MedicineId,
                    MedicineName = g.Key.MedicineName,
                    TotalQuantity = g.Sum(pd => pd.Quantity),
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToListAsync();

            // Top bác sĩ kê đơn nhiều nhất
            var topDoctors = await _context
                .Prescriptions.Where(p => p.PrescriptionDate >= startDate)
                .GroupBy(p => new
                {
                    p.Staff.UserId,
                    p.Staff.FullName,
                    p.Staff.MedicalStaff.Specialty,
                })
                .Select(g => new TopDoctorDTO
                {
                    StaffId = g.Key.UserId,
                    FullName = g.Key.FullName,
                    Specialty = g.Key.Specialty,
                    PrescriptionCount = g.Count(),
                })
                .OrderByDescending(x => x.PrescriptionCount)
                .Take(10)
                .ToListAsync();

            // Thuốc bán chạy nhất (từ invoice details)
            var bestSellingMedicines = await _context
                .InvoiceDetails.Where(id =>
                    id.MedicineId != null
                    && id.Invoice.Status == "Paid"
                    && id.Invoice.InvoiceDate >= startDate
                )
                .GroupBy(id => new { id.MedicineId, id.Medicine.MedicineName })
                .Select(g => new BestSellingMedicineDTO
                {
                    MedicineId = g.Key.MedicineId.Value,
                    MedicineName = g.Key.MedicineName,
                    TotalSold = g.Sum(id => id.Quantity),
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToListAsync();

            var analytics = new PrescriptionAnalyticsDTO
            {
                Success = true,
                TopMedicines = topMedicines,
                TopDoctors = topDoctors,
                BestSellingMedicines = bestSellingMedicines,
            };

            return new ResponseValue<PrescriptionAnalyticsDTO>(
                analytics,
                StatusReponse.Success,
                "Lấy phân tích đơn thuốc thành công."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy phân tích đơn thuốc");
            return new ResponseValue<PrescriptionAnalyticsDTO>(
                new PrescriptionAnalyticsDTO { Success = false },
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy phân tích đơn thuốc: " + ex.Message
            );
        }
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

// DTO cho Revenue Forecast
public class RevenueForecastDTO
{
    public List<RevenueHistoryItemDTO> Historical { get; set; } = new();
    public RevenuePredictionDTO Forecast { get; set; } = new();
}

public class RevenueHistoryItemDTO
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Revenue { get; set; }
}

public class RevenuePredictionDTO
{
    public decimal PredictedRevenue { get; set; }
    public decimal Confidence { get; set; }
    public int NextMonth { get; set; }
    public int NextYear { get; set; }
}

public class RevenueHistoryDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
}

// DTO cho Prescription Analytics
public class PrescriptionAnalyticsDTO
{
    public bool Success { get; set; }
    public List<TopMedicineDTO> TopMedicines { get; set; } = new();
    public List<TopDoctorDTO> TopDoctors { get; set; } = new();
    public List<BestSellingMedicineDTO> BestSellingMedicines { get; set; } = new();
}

public class TopMedicineDTO
{
    public int? MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
}

public class TopDoctorDTO
{
    public int StaffId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public int PrescriptionCount { get; set; }
}

public class BestSellingMedicineDTO
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
}

// DTO cho Combined Revenue Report
public class CombinedRevenueReportDTO
{
    public decimal TotalRevenue { get; set; }
    public List<RevenueByDateDTO> RevenueByDate { get; set; } = new();
    public int TotalAppointmentsToday { get; set; }
    public int CompletedAppointmentsToday { get; set; }
    public int PendingInvoicesCount { get; set; }
}
