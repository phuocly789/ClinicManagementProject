using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IMedicineService
{
    Task<ResponseValue<PagedResult<MedicineDTO>>> GetAllMedicinesAsync(
        string? search,
        string? type, // Thêm
        string? unit, // Thêm
        decimal? minPrice, // Thêm
        decimal? maxPrice, // Thêm
        bool lowStock, // Thêm
        int page,
        int pageSize
    );
    Task<ResponseValue<MedicineDTO>> GetMedicineByIdAsync(int medicineId);
    Task<ResponseValue<MedicineDTO>> CreateMedicineAsync(MedicineDTO request);
    Task<ResponseValue<MedicineDTO>> UpdateMedicineAsync(int medicineId, MedicineDTO request);

    Task DeleteMedicineAsync(int medicineId);

    //for dashboard
    Task<List<LowStockMedicineDTO>> GetLowStockMedicinesAsync(int threshold);
}

public class MedicineService : IMedicineService
{
    private readonly IUnitOfWork _uow;
    private readonly IMedicineRepository _medicineRepository;
    private readonly ILogger<MedicineService> _logger;

    public MedicineService(
        IUnitOfWork uow,
        IMedicineRepository repository,
        ILogger<MedicineService> logger
    )
    {
        _uow = uow;
        _medicineRepository = repository;
        _logger = logger;
    }

    public async Task<ResponseValue<PagedResult<MedicineDTO>>> GetAllMedicinesAsync(
        string? search = null,
        string? type = null,
        string? unit = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool lowStock = false,
        int page = 1,
        int pageSize = 10
    )
    {
        try
        {
            var query = _medicineRepository.GetAll().AsNoTracking();
            // search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.MedicineName != null && EF.Functions.Like(s.MedicineName, $"%{search}%")
                );
            }
            // filter by type
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(m => m.MedicineType == type);
            }
            // filter by unit
            if (!string.IsNullOrEmpty(unit))
            {
                query = query.Where(m => m.Unit == unit);
            }
            // filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(m => m.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(m => m.Price <= maxPrice.Value);
            }
            // filter by low stock (ví dụ: dưới 100)
            if (lowStock)
            {
                query = query.Where(m => m.StockQuantity < 300);
            }

            //get total count
            var totalItems = await query.CountAsync();

            //fetch services with pagination
            var medicines = await query
                .OrderBy(m => m.MedicineId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MedicineDTO
                {
                    MedicineId = m.MedicineId,
                    MedicineName = m.MedicineName,
                    MedicineType = m.MedicineType,
                    Unit = m.Unit,
                    Price = m.Price,
                    StockQuantity = m.StockQuantity,
                    Description = m.Description,
                })
                .ToListAsync();
            return new ResponseValue<PagedResult<MedicineDTO>>(
                new PagedResult<MedicineDTO>
                {
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    Items = medicines,
                },
                StatusReponse.Success,
                "Lấy danh sách thuốc thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<PagedResult<MedicineDTO>>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy danh sách thuốc: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<MedicineDTO>> GetMedicineByIdAsync(int medicineId)
    {
        try
        {
            var medicine = await _medicineRepository.GetByIdAsync(medicineId);
            if (medicine == null)
            {
                return new ResponseValue<MedicineDTO>(
                    null,
                    StatusReponse.NotFound,
                    "Medicine not found"
                );
            }
            return new ResponseValue<MedicineDTO>(
                new MedicineDTO
                {
                    MedicineId = medicine.MedicineId,
                    MedicineName = medicine.MedicineName,
                    MedicineType = medicine.MedicineType,
                    Unit = medicine.Unit,
                    Price = medicine.Price,
                    StockQuantity = medicine.StockQuantity,
                    Description = medicine.Description,
                },
                StatusReponse.Success,
                "Lấy thuốc thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<MedicineDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy thuốc: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<MedicineDTO>> CreateMedicineAsync(MedicineDTO request)
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(request.MedicineName)
                || string.IsNullOrWhiteSpace(request.MedicineType)
                || request.Price <= 0
            )
            {
                return new ResponseValue<MedicineDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thông tin thuốc không hợp lệ."
                );
            }

            //check duplicate
            if (
                await _medicineRepository
                    .GetAll()
                    .AnyAsync(m => m.MedicineName == request.MedicineName)
            )
            {
                return new ResponseValue<MedicineDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Tên thuốc đã tồn tại."
                );
            }
            using var transaction = await _uow.BeginTransactionAsync();

            var medicine = new Medicine
            {
                MedicineName = request.MedicineName,
                MedicineType = request.MedicineType,
                Unit = request.Unit,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Description = request.Description,
            };
            //lưu
            await _medicineRepository.AddAsync(medicine);
            //load lại csdl
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();
            //trả về kết quả
            return new ResponseValue<MedicineDTO>(
                new MedicineDTO
                {
                    MedicineId = medicine.MedicineId,
                    MedicineName = medicine.MedicineName,
                    MedicineType = medicine.MedicineType,
                    Unit = medicine.Unit,
                    Price = medicine.Price,
                    StockQuantity = medicine.StockQuantity,
                    Description = medicine.Description,
                },
                StatusReponse.Success,
                "Tạo thuốc thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<MedicineDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi tạo thuốc: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<MedicineDTO>> UpdateMedicineAsync(
        int medicineId,
        MedicineDTO request
    )
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(request.MedicineName)
                || string.IsNullOrWhiteSpace(request.MedicineType)
                || request.Price <= 0
            )
            {
                return new ResponseValue<MedicineDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thông tin thuốc không hợp lệ."
                );
            }
            //get medicine
            var medicine = await _medicineRepository.GetByIdAsync(medicineId);
            //check duplicate
            var duplicateMedicine = await _medicineRepository
                .GetAll()
                .AnyAsync(m =>
                    m.MedicineName.ToLower() == request.MedicineName.ToLower()
                    && m.MedicineId != medicineId
                );
            if (duplicateMedicine)
            {
                return new ResponseValue<MedicineDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Tên thuốc đã tồn tại."
                );
            }
            //cập nhật
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                medicine.MedicineName = request.MedicineName;
                medicine.MedicineType = request.MedicineType;
                medicine.Unit = request.Unit;
                medicine.Price = request.Price;
                medicine.StockQuantity = request.StockQuantity;
                medicine.Description = request.Description;

                await _medicineRepository.Update(medicine);

                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseValue<MedicineDTO>(
                    new MedicineDTO
                    {
                        MedicineId = medicine.MedicineId,
                        MedicineName = medicine.MedicineName,

                        MedicineType = medicine.MedicineType,
                        Unit = medicine.Unit,
                        Price = medicine.Price,
                        StockQuantity = medicine.StockQuantity,
                        Description = medicine.Description,
                    },
                    StatusReponse.Success,
                    "Cập nhật thuốc thành công."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseValue<MedicineDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi cập nhật thuốc: " + ex.Message
                );
            }
        }
        catch (Exception ex)
        {
            return new ResponseValue<MedicineDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi cập nhật thuốc: " + ex.Message
            );
        }
    }

    public async Task DeleteMedicineAsync(int medicineId)
    {
        try
        {
            var medicine = await _medicineRepository.GetByIdAsync(medicineId);
            if (medicine == null)
            {
                _logger.LogWarning("Không tìm thấy thuốc với medicineId: {MedicineId}", medicineId);
            }
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                await _medicineRepository.DeleteAsync(medicineId);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xóa thuốc với medicineId: {MedicineId}", medicineId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa dịch vụ với serviceId: {ServiceId}", medicineId);
            throw;
        }
    }

    //for dashboard
    public async Task<List<LowStockMedicineDTO>> GetLowStockMedicinesAsync(int threshold)
    {
        return await _medicineRepository
            .GetAll()
            .Where(m => m.StockQuantity < threshold)
            .OrderBy(m => m.StockQuantity)
            .Select(m => new LowStockMedicineDTO
            {
                MedicineId = m.MedicineId,
                MedicineName = m.MedicineName,
                StockQuantity = m.StockQuantity,
                Unit = m.Unit,
            })
            .ToListAsync();
    }
}
