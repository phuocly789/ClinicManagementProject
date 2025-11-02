using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface ISuplierService
{
    Task<ResponseValue<PagedResult<SupplierDTO>>> GetAllMSupliersAsync(
        string? search = null,
        int? page = null,
        int? pageSize = null
    );
    Task<ResponseValue<SupplierDTO>> CreateSuplierAsync(SupplierDTO request);
    Task<ResponseValue<SupplierDTO>> UpdateSuplierAsync(int suplierId, SupplierDTO request);
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

    public async Task<ResponseValue<PagedResult<SupplierDTO>>> GetAllMSupliersAsync(
        string? search = null,
        int? page = null,
        int? pageSize = null
    )
    {
        try
        {
            var query = _suplierRepository.GetAll().AsNoTracking();

            // search theo tên
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.SupplierName != null && EF.Functions.Like(s.SupplierName, $"%{search}%")
                );
            }

            var totalItems = await query.CountAsync();

            // ✅ Nếu không truyền page hoặc pageSize -> lấy toàn bộ
            if (page == null || pageSize == null || page <= 0 || pageSize <= 0)
            {
                var allSupliers = await query
                    .OrderBy(s => s.SupplierId)
                    .Select(s => new SupplierDTO
                    {
                        SupplierId = s.SupplierId,
                        SupplierName = s.SupplierName,
                        ContactEmail = s.ContactEmail,
                        ContactPhone = s.ContactPhone,
                        Address = s.Address,
                        Description = s.Description,
                    })
                    .ToListAsync();

                return new ResponseValue<PagedResult<SupplierDTO>>(
                    new PagedResult<SupplierDTO>
                    {
                        TotalItems = totalItems,
                        Page = 0,
                        PageSize = 0,
                        Items = allSupliers,
                    },
                    StatusReponse.Success,
                    "Lấy toàn bộ danh sách nhà cung cấp thành công."
                );
            }

            // ✅ Ngược lại: phân trang
            var supliers = await query
                .OrderBy(s => s.SupplierId)
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(s => new SupplierDTO
                {
                    SupplierId = s.SupplierId,
                    SupplierName = s.SupplierName,
                    ContactEmail = s.ContactEmail,
                    ContactPhone = s.ContactPhone,
                    Address = s.Address,
                    Description = s.Description,
                })
                .ToListAsync();

            return new ResponseValue<PagedResult<SupplierDTO>>(
                new PagedResult<SupplierDTO>
                {
                    TotalItems = totalItems,
                    Page = page.Value,
                    PageSize = pageSize.Value,
                    Items = supliers,
                },
                StatusReponse.Success,
                "Lấy danh sách nhà cung cấp thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<PagedResult<SupplierDTO>>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy danh sách nhà cung cấp: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<SupplierDTO>> CreateSuplierAsync(SupplierDTO request)
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(request.SupplierName)
                || string.IsNullOrWhiteSpace(request.ContactEmail)
                || string.IsNullOrWhiteSpace(request.ContactPhone)
                || string.IsNullOrWhiteSpace(request.Address)
            )
            {
                return new ResponseValue<SupplierDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thông tin thuốc không hợp lệ."
                );
            }
            //check duplicate
            if (
                await _suplierRepository
                    .GetAll()
                    .AnyAsync(s => s.SupplierName == request.SupplierName)
            )
            {
                return new ResponseValue<SupplierDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Tên thuốc đã tồn tại."
                );
            }
            //transaction
            using var transaction = await _uow.BeginTransactionAsync();

            var suplier = new Supplier
            {
                SupplierName = request.SupplierName,
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
            return new ResponseValue<SupplierDTO>(
                new SupplierDTO
                {
                    SupplierId = suplier.SupplierId,
                    SupplierName = suplier.SupplierName,
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
            return new ResponseValue<SupplierDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi tạo thuốc: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<SupplierDTO>> UpdateSuplierAsync(
        int suplierId,
        SupplierDTO request
    )
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(request.SupplierName)
                || string.IsNullOrWhiteSpace(request.ContactEmail)
                || string.IsNullOrWhiteSpace(request.ContactPhone)
                || string.IsNullOrWhiteSpace(request.Address)
            )
            {
                return new ResponseValue<SupplierDTO>(
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
                    m.SupplierName.ToLower() == request.SupplierName.ToLower()
                    && m.SupplierId != suplierId
                );
            if (duplicateSuplier)
            {
                return new ResponseValue<SupplierDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Tên thuốc đã tồn tại."
                );
            }
            //cập nhật
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                suplier.SupplierName = request.SupplierName;
                suplier.ContactEmail = request.ContactEmail;
                suplier.ContactPhone = request.ContactPhone;
                suplier.Address = request.Address;
                suplier.Description = request.Description;

                await _suplierRepository.Update(suplier);

                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseValue<SupplierDTO>(
                    new SupplierDTO
                    {
                        SupplierName = suplier.SupplierName,
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
                return new ResponseValue<SupplierDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi cập nhật thuốc: " + ex.Message
                );
            }
        }
        catch (Exception ex)
        {
            return new ResponseValue<SupplierDTO>(
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
