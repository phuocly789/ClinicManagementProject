using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IImportService
{
    Task<ResponseValue<PagedResult<ImportDTO>>> GetAllImportBillsAsync(
        int? supplierId = null,
        int page = 1,
        int pageSize = 10
    );
    Task<ResponseValue<ImportDetailByIdDTO>> GetImportBillByIdAsync(int importId);
    Task<ResponseValue<ImportDTO>> CreateImportBillAsync(ImportCreateDTO request);
}

public class ImportService : IImportService
{
    private readonly UnitOfWork _uow;
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
        int? supplierId = null,
        int page = 1,
        int pageSize = 10
    )
    {
        try
        {
            //fetch data
            var query = _importBillRepository.GetAll().AsNoTracking();

            //apply suplier filter if provide
            if (supplierId.HasValue && supplierId.Value > 0)
            {
                query = query.Where(ib => ib.SupplierId == supplierId.Value);
            }

            //get total count
            var totalItems = await query.CountAsync();

            //fetch import with panigation
            var importBill = await query
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
                    Items = importBill,
                },
                StatusReponse.Success,
                "Lấy danh sách nhập hàng thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<PagedResult<ImportDTO>>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy danh sách nhập hàng: " + ex.Message
            );
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
