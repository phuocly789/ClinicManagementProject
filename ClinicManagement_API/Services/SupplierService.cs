using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface ISuplierService
{
    Task<ResponseValue<PagedResult<SuplierDTO>>> GetAllMSupliersAsync(
        string? search = null,
        int page = 1,
        int pageSize = 10
    );
    Task<ResponseValue<SuplierDTO>> CreateSuplierAsync(SuplierDTO request);
    Task<ResponseValue<SuplierDTO>> UpdateSuplierAsync(int suplierId, SuplierDTO request);
    Task DeleteSuplierAsync(int suplierId);
}

public class SuplierService : ISuplierService
{
    private readonly ISupplierRepository _suplierRepository;
    private readonly ILogger<SuplierService> _logger;
    private readonly IUnitOfWork _uow;

    public SuplierService(
        IUnitOfWork uow,
        ISupplierRepository suplierRepository,
        ILogger<SuplierService> logger
    )
    {
        _uow = uow;
        _suplierRepository = suplierRepository;
        _logger = logger;
    }

    public async Task<ResponseValue<PagedResult<SuplierDTO>>> GetAllMSupliersAsync(
        string? search = null,
        int page = 1,
        int pageSize = 10
    )
    {
        try
        {
            var query = _suplierRepository.GetAll().AsNoTracking();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.SupplierName != null && EF.Functions.Like(s.SupplierName, $"%{search}%")
                );
            }
            //
            var totalItems = await query.CountAsync();

            //fetch services with pagination
            var supliers = await query
                .OrderBy(s => s.SupplierId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SuplierDTO
                {
                    SuplierId = s.SupplierId,
                    SuplierName = s.SupplierName,
                    ContactEmail = s.ContactEmail,
                    ContactPhone = s.ContactPhone,
                    Address = s.Address,
                    Description = s.Description,
                })
                .ToListAsync();
            return new ResponseValue<PagedResult<SuplierDTO>>(
                new PagedResult<SuplierDTO>
                {
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    Items = supliers,
                },
                StatusReponse.Success,
                "Lấy danh sách thuốc thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<PagedResult<SuplierDTO>>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy danh sách thuốc: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<SuplierDTO>> CreateSuplierAsync(SuplierDTO request)
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(request.SuplierName)
                || string.IsNullOrWhiteSpace(request.ContactEmail)
                || string.IsNullOrWhiteSpace(request.ContactPhone)
                || string.IsNullOrWhiteSpace(request.Address)
            )
            {
                return new ResponseValue<SuplierDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thông tin thuốc không hợp lệ."
                );
            }
            //check duplicate
            if (
                await _suplierRepository
                    .GetAll()
                    .AnyAsync(s => s.SupplierName == request.SuplierName)
            )
            {
                return new ResponseValue<SuplierDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Tên thuốc đã tồn tại."
                );
            }
            //transaction
            using var transaction = await _uow.BeginTransactionAsync();

            var suplier = new Supplier
            {
                SupplierName = request.SuplierName,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                Address = request.Address,
                Description = request.Description,
            };

            //lưu
            await _suplierRepository.AddAsync(suplier);
            //load lại csdl
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();
            //trả về kết quả
            return new ResponseValue<SuplierDTO>(
                new SuplierDTO
                {
                    SuplierId = suplier.SupplierId,
                    SuplierName = suplier.SupplierName,
                    ContactEmail = suplier.ContactEmail,
                    ContactPhone = suplier.ContactPhone,
                    Address = suplier.Address,
                    Description = suplier.Description,
                },
                StatusReponse.Success,
                "Tạo thuốc thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<SuplierDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi tạo thuốc: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<SuplierDTO>> UpdateSuplierAsync(
        int suplierId,
        SuplierDTO request
    )
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(request.SuplierName)
                || string.IsNullOrWhiteSpace(request.ContactEmail)
                || string.IsNullOrWhiteSpace(request.ContactPhone)
                || string.IsNullOrWhiteSpace(request.Address)
            )
            {
                return new ResponseValue<SuplierDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thông tin thuốc không hợp lệ."
                );
            }
            //get suplier
            var suplier = await _suplierRepository.GetByIdAsync(suplierId);
            //check duplicate
            var duplicateSuplier = await _suplierRepository
                .GetAll()
                .AnyAsync(m =>
                    m.SupplierName.ToLower() == request.SuplierName.ToLower()
                    && m.SupplierId != suplierId
                );
            if (duplicateSuplier)
            {
                return new ResponseValue<SuplierDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Tên thuốc đã tồn tại."
                );
            }
            //cập nhật
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                suplier.SupplierName = request.SuplierName;
                suplier.ContactEmail = request.ContactEmail;
                suplier.ContactPhone = request.ContactPhone;
                suplier.Address = request.Address;
                suplier.Description = request.Description;

                await _suplierRepository.Update(suplier);

                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseValue<SuplierDTO>(
                    new SuplierDTO
                    {
                        SuplierName = suplier.SupplierName,
                        ContactEmail = suplier.ContactEmail,
                        ContactPhone = suplier.ContactPhone,
                        Address = suplier.Address,
                        Description = suplier.Description,
                    },
                    StatusReponse.Success,
                    "Cập nhật thuốc thành công."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseValue<SuplierDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi cập nhật thuốc: " + ex.Message
                );
            }
        }
        catch (Exception ex)
        {
            return new ResponseValue<SuplierDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi cập nhật thuốc: " + ex.Message
            );
        }
    }

    public async Task DeleteSuplierAsync(int suplierId)
    {
        try
        {
            var suplier = await _suplierRepository.GetByIdAsync(suplierId);
            if (suplier == null)
            {
                _logger.LogWarning("Không tìm thấy thuốc với suplierId: {SuplierId}", suplierId);
            }
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                await _suplierRepository.DeleteAsync(suplierId);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xóa thuốc với suplierId: {SuplierId}", suplierId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa dịch vụ với serviceId: {ServiceId}", suplierId);
            throw;
        }
    }
}
