using ClinicManagement_Infrastructure.Data.Models;
using ClinicManagement_Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

public interface IImportService
{
    Task<ResponseValue<PagedResult<ImportDTO>>> GetAllImportBillsAsync(
        string? search = null, // Thêm
        int? supplierId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal? minTotal = null, // Thêm
        decimal? maxTotal = null, // Thêm
        int page = 1,
        int pageSize = 10
    );
    Task<ResponseValue<ImportDetailByIdDTO>> GetImportBillByIdAsync(int importId);
    Task<ResponseValue<ImportDTO>> CreateImportBillAsync(ImportCreateDTO request);
    Task<ResponseValue<ImportReportDTO>> GenerateImportReportAsync(
        int? supplierId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 10
    );
}

public class ImportService : IImportService
{
    private readonly IUnitOfWork _uow;
    private readonly IImportDetailRepository _importDetailRepository;
    private readonly IImportBillRepository _importBillRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMedicineRepository _medicineRepository;
    private readonly ILogger<ImportService> _logger;

    public ImportService(
        UnitOfWork uow,
        IImportDetailRepository importDetailRepository,
        IImportBillRepository importBillRepository,
        ISupplierRepository supplierRepository,
        IMedicineRepository medicineRepository,
        ILogger<ImportService> logger
    )
    {
        _uow = uow;
        _importDetailRepository = importDetailRepository;
        _importBillRepository = importBillRepository;
        _supplierRepository = supplierRepository;
        _medicineRepository = medicineRepository;
        _logger = logger;
    }

    public async Task<ResponseValue<PagedResult<ImportDTO>>> GetAllImportBillsAsync(
        string? search = null, // Thêm
        int? supplierId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        decimal? minTotal = null, // Thêm
        decimal? maxTotal = null, // Thêm
        int page = 1,
        int pageSize = 10
    )
    {
        try
        {
            var query = _importBillRepository.GetAll().AsNoTracking();

            // Thêm bộ lọc tìm kiếm theo tên nhà cung cấp
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(ib =>
                    ib.Supplier != null && ib.Supplier.SupplierName.Contains(search)
                );
            }

            if (supplierId.HasValue && supplierId.Value > 0)
                query = query.Where(ib => ib.SupplierId == supplierId.Value);
            if (startDate.HasValue)
                query = query.Where(ib => ib.ImportDate >= startDate.Value);
            if (endDate.HasValue)
            {
                // Nếu bạn muốn endDate tính đến cuối ngày
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(ib => ib.ImportDate <= end);
            }

            // Thêm bộ lọc theo tổng tiền
            if (minTotal.HasValue)
                query = query.Where(ib => ib.TotalAmount >= minTotal.Value);
            if (maxTotal.HasValue)
                query = query.Where(ib => ib.TotalAmount <= maxTotal.Value);

            // Tổng số lượng
            var totalItems = await query.CountAsync();

            // Lấy danh sách hóa đơn nhập với phân trang
            var importBills = await query
                .OrderBy(ib => ib.ImportId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ib => new ImportDTO
                {
                    ImportId = ib.ImportId,
                    SupplierId = ib.SupplierId,
                    SupplierName = ib.Supplier != null ? ib.Supplier.SupplierName : string.Empty,
                    ImportDate = ib.ImportDate,
                    TotalAmount = ib.TotalAmount,
                    Notes = ib.Notes,
                    CreatedBy = ib.CreatedBy,
                })
                .ToListAsync();

            return new ResponseValue<PagedResult<ImportDTO>>(
                new PagedResult<ImportDTO>
                {
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    Items = importBills,
                },
                StatusReponse.Success,
                "Lấy danh sách nhập hàng thành công."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách nhập hàng.");
            return new ResponseValue<PagedResult<ImportDTO>>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy danh sách nhập hàng: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<ImportReportDTO>> GenerateImportReportAsync(
        int? supplierId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 10
    )
    {
        try
        {
            var context = _uow.GetDbContext();

            // Truy vấn dữ liệu từ các bảng
            var query =
                from ib in context.ImportBills
                join s in context.Suppliers on ib.SupplierId equals s.SupplierId into supplierLeft
                from s in supplierLeft.DefaultIfEmpty()
                join id in context.ImportDetails on ib.ImportId equals id.ImportId into detailsLeft
                from id in detailsLeft.DefaultIfEmpty()
                join m in context.Medicines on id.MedicineId equals m.MedicineId into medicineLeft
                from m in medicineLeft.DefaultIfEmpty()
                select new ImportReportView
                {
                    ImportId = ib.ImportId,
                    SupplierId = ib.SupplierId,
                    SupplierName = s.SupplierName,
                    ImportDate = ib.ImportDate,
                    TotalAmount = ib.TotalAmount,
                    Notes = ib.Notes,
                    CreatedBy = ib.CreatedBy,
                    ImportDetailId = id.ImportDetailId,
                    MedicineId = id.MedicineId,
                    MedicineName = m.MedicineName,
                    Quantity = id.Quantity,
                    ImportPrice = id.ImportPrice,
                    SubTotal = id.Quantity * id.ImportPrice,
                };

            // Áp dụng bộ lọc
            if (supplierId.HasValue && supplierId.Value > 0)
                query = query.Where(r => r.SupplierId == supplierId.Value);
            if (startDate.HasValue)
                query = query.Where(r => r.ImportDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.ImportDate <= endDate.Value);

            // Tính toán tóm tắt
            var totalBills = await query.Select(r => r.ImportId).Distinct().CountAsync();
            var totalAmount = await query.Select(r => r.TotalAmount).Distinct().SumAsync();
            var totalQuantity = await query.SumAsync(r => r.Quantity ?? 0);
            var supplierCounts = await query
                .GroupBy(r => new { r.SupplierId, r.SupplierName })
                .Select(g => new
                {
                    g.Key.SupplierId,
                    g.Key.SupplierName,
                    Count = g.Select(r => r.ImportId).Distinct().Count(),
                })
                .ToDictionaryAsync(g => g.SupplierName ?? "Unknown", g => g.Count);

            // Lấy danh sách chi tiết với phân trang
            var totalItems = await query.Select(r => r.ImportId).Distinct().CountAsync();
            var items = await query
                .GroupBy(r => new
                {
                    r.ImportId,
                    r.SupplierId,
                    r.SupplierName,
                    r.ImportDate,
                    r.TotalAmount,
                    r.Notes,
                    r.CreatedBy,
                })
                .OrderBy(g => g.Key.ImportId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new ImportReportItemDTO
                {
                    ImportId = g.Key.ImportId,
                    SupplierId = g.Key.SupplierId ?? 0,
                    SupplierName = g.Key.SupplierName ?? string.Empty,
                    ImportDate = g.Key.ImportDate,
                    TotalAmount = g.Key.TotalAmount,
                    Notes = g.Key.Notes ?? string.Empty,
                    CreatedBy = g.Key.CreatedBy ?? 0,
                    Details = query
                        .Where(d => d.ImportId == g.Key.ImportId)
                        .Select(d => new ImportDetailItemDTO
                        {
                            MedicineId = d.MedicineId ?? 0,
                            MedicineName = d.MedicineName ?? string.Empty,
                            Quantity = d.Quantity ?? 0,
                            ImportPrice = d.ImportPrice ?? 0,
                            SubTotal = d.SubTotal ?? 0,
                        })
                        .ToList(),
                })
                .ToListAsync();

            var report = new ImportReportDTO
            {
                TotalBills = totalBills,
                TotalAmount = totalAmount,
                TotalQuantity = totalQuantity,
                SupplierCounts = supplierCounts,
                Details = new PagedResult<ImportReportItemDTO>
                {
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    Items = items,
                },
            };

            return new ResponseValue<ImportReportDTO>
            {
                Content = report,
                Status = StatusReponse.Success,
                Message = "Tạo báo cáo nhập hàng thành công.",
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo báo cáo nhập hàng.");
            return new ResponseValue<ImportReportDTO>
            {
                Status = StatusReponse.Error,
                Message = "Đã xảy ra lỗi khi tạo báo cáo nhập hàng: " + ex.Message,
            };
        }
    }

    public async Task<ResponseValue<ImportDetailByIdDTO>> GetImportBillByIdAsync(int importId)
    {
        try
        {
            var importBill = await _importBillRepository.GetByIdWithDetailsAsync(importId);
            if (importBill == null)
            {
                return new ResponseValue<ImportDetailByIdDTO>(
                    null,
                    StatusReponse.NotFound,
                    "Không tìm thấy nhập hàng."
                );
            }
            return new ResponseValue<ImportDetailByIdDTO>(
                new ImportDetailByIdDTO
                {
                    ImportId = importBill.ImportId,
                    SupplierId = importBill.SupplierId,
                    SupplierName =
                        importBill.Supplier != null
                            ? importBill.Supplier.SupplierName
                            : string.Empty,
                    ImportDate = importBill.ImportDate,
                    TotalAmount = importBill.TotalAmount,
                    Notes = importBill.Notes,
                    Details = importBill
                        .ImportDetails.Select(id => new ImportDetailItemDTO
                        {
                            MedicineId = id.MedicineId,
                            MedicineName = id.Medicine.MedicineName,
                            Quantity = id.Quantity,
                            ImportPrice = id.ImportPrice,
                            SubTotal = id.Quantity * id.ImportPrice,
                        })
                        .ToList(),
                },
                StatusReponse.Success,
                "Lấy nhập hàng thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<ImportDetailByIdDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy nhập hàng: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<ImportDTO>> CreateImportBillAsync(ImportCreateDTO request)
    {
        try
        {
            if (
                request.SupplierId <= 0
                || request.CreatedBy <= 0
                || request.Details == null
                || !request.Details.Any()
            )
            {
                return new ResponseValue<ImportDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thông tin nhập hàng không hợp lệ."
                );
            }
            //check sự tồn tại của nhà cung cấp
            if (
                !await _supplierRepository
                    .GetAll()
                    .AnyAsync(s => s.SupplierId == request.SupplierId)
            )
            {
                return new ResponseValue<ImportDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Nhà cung cấp không tồn tại."
                );
            }
            //check if all medician exits
            foreach (var detail in request.Details)
            {
                if (detail.Quantity <= 0 || detail.ImportPrice <= 0)
                {
                    return new ResponseValue<ImportDTO>(
                        null,
                        StatusReponse.BadRequest,
                        "Thông tin chi tiết nhập hàng không hợp lệ."
                    );
                }
                if (
                    !await _medicineRepository
                        .GetAll()
                        .AnyAsync(m => m.MedicineId == detail.MedicineId)
                )
                {
                    return new ResponseValue<ImportDTO>(
                        null,
                        StatusReponse.BadRequest,
                        "Thuốc không tồn tại."
                    );
                }
            }
            //calculate TotalAmount
            var TotalAmount = request.Details.Sum(d => d.Quantity * d.ImportPrice);

            //create import bill
            using var transaction = await _uow.BeginTransactionAsync();
            var importBill = new ImportBill
            {
                SupplierId = request.SupplierId,
                ImportDate = DateTime.Now,
                TotalAmount = TotalAmount,
                Notes = request.Notes,
                CreatedBy = request.CreatedBy,
                ImportDetails = request
                    .Details.Select(d => new ImportDetail
                    {
                        MedicineId = d.MedicineId,
                        Quantity = d.Quantity,
                        ImportPrice = d.ImportPrice,
                    })
                    .ToList(),
            };

            try
            {
                //add import bill
                await _importBillRepository.AddAsync(importBill);
                //update stock
                foreach (var detail in request.Details)
                {
                    await UpdateStockQuantityAsync(detail.MedicineId, detail.Quantity);
                }
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseValue<ImportDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi tạo nhập hàng: " + ex.Message
                );
            }
            return new ResponseValue<ImportDTO>(
                new ImportDTO
                {
                    ImportId = importBill.ImportId,
                    SupplierId = importBill.SupplierId,
                    SupplierName =
                        importBill.Supplier != null
                            ? importBill.Supplier.SupplierName
                            : string.Empty,
                    ImportDate = importBill.ImportDate,
                    TotalAmount = importBill.TotalAmount,
                    Notes = importBill.Notes,
                    CreatedBy = importBill.CreatedBy,
                },
                StatusReponse.Success,
                "Tạo nhập hàng thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<ImportDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi tạo nhập hàng: " + ex.Message
            );
        }
    }

    public async Task UpdateStockQuantityAsync(int medicineId, int quantity)
    {
        var medicine = await _medicineRepository
            .GetAll()
            .FirstOrDefaultAsync(m => m.MedicineId == medicineId);
        if (medicine != null)
        {
            medicine.StockQuantity += quantity;
            await _medicineRepository.Update(medicine);
            await _uow.SaveChangesAsync();
        }
    }
}
