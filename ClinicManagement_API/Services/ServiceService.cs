using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public interface IServiceService
{
    Task<ResponseValue<PagedResult<ServiceDTO>>> GetAllServicesAsync(
        string search,
        int page,
        int pageSize
    );
    Task<ResponseValue<ServiceDTO>> GetServiceByIdAsync(int serviceId);
    Task<ResponseValue<ServiceDTO>> CreateServiceAsync(ServiceDTO request);
    Task<ResponseValue<ServiceDTO>> UpdateServiceAsync(int serviceId, ServiceDTO request);

    Task DeleteServiceAsync(int serviceId);
}

public class ServiceService : IServiceService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ServiceService> _logger;
    private readonly IServiceRepository _serviceRepository;

    public ServiceService(
        IUnitOfWork uow,
        ILogger<ServiceService> logger,
        IServiceRepository serviceRepository
    )
    {
        _uow = uow;
        _logger = logger;
        _serviceRepository = serviceRepository;
    }

    public async Task<ResponseValue<PagedResult<ServiceDTO>>> GetAllServicesAsync(
        string search,
        int page = 1,
        int pageSize = 10
    )
    {
        try
        {
            var query = _serviceRepository.GetAll().AsNoTracking();
            //search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.ServiceName != null && EF.Functions.Like(s.ServiceName, $"%{search}%")
                );
            }

            //get total count
            var totalItems = await query.CountAsync();
            //fetch services with pagination
            var services = await query
                .OrderBy(s => s.ServiceId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ServiceDTO
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    ServiceType = s.ServiceType,
                    Price = s.Price,
                    Description = s.Description,
                })
                .ToListAsync();
            return new ResponseValue<PagedResult<ServiceDTO>>(
                new PagedResult<ServiceDTO>
                {
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    Items = services,
                },
                StatusReponse.Success,
                "Lấy danh sách dịch vụ thành công."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Lỗi khi lấy danh sách dịch vụ với tìm kiếm: {Search}, trang: {Page}, kích thước trang: {PageSize}",
                search ?? "none",
                page,
                pageSize
            );
            throw;
        }
    }

    public async Task<ResponseValue<ServiceDTO>> GetServiceByIdAsync(int serviceId)
    {
        try
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null)
            {
                return new ResponseValue<ServiceDTO>(
                    null,
                    StatusReponse.NotFound,
                    "Service not found"
                );
            }
            return new ResponseValue<ServiceDTO>(
                new ServiceDTO
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    ServiceType = service.ServiceType,
                    Price = service.Price,
                    Description = service.Description,
                },
                StatusReponse.Success,
                "Lấy dịch vụ thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<ServiceDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy dịch vụ: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<ServiceDTO>> CreateServiceAsync(ServiceDTO request)
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(request.ServiceName)
                || string.IsNullOrWhiteSpace(request.ServiceType)
                || request.Price <= 0
            )
            {
                return new ResponseValue<ServiceDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thông tin dịch vụ không hợp lệ."
                );
            }

            //check for duplicate service name
            if (
                await _serviceRepository
                    .GetAll()
                    .AnyAsync(s => s.ServiceName == request.ServiceName)
            )
            {
                return new ResponseValue<ServiceDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Tên dịch vụ đã tồn tại."
                );
            }
            using var transaction = await _uow.BeginTransactionAsync();
            var service = new Service
            {
                ServiceName = request.ServiceName,
                ServiceType = request.ServiceType,
                Price = request.Price,
                Description = request.Description,
            };
            await _serviceRepository.AddAsync(service);
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();
            return new ResponseValue<ServiceDTO>(
                new ServiceDTO
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    ServiceType = service.ServiceType,
                    Price = service.Price,
                    Description = service.Description,
                },
                StatusReponse.Success,
                "Tạo dịch vụ thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<ServiceDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi tạo dịch vụ: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<ServiceDTO>> UpdateServiceAsync(
        int serviceId,
        ServiceDTO request
    )
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(request.ServiceName)
                || string.IsNullOrWhiteSpace(request.ServiceType)
                || request.Price <= 0
            )
            {
                return new ResponseValue<ServiceDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Thông tin dịch vụ không hợp lệ."
                );
            }
            //lấy service đó
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            //check duplicate
            var duplicateService = await _serviceRepository
                .GetAll()
                .AnyAsync(s =>
                    s.ServiceName.ToLower() == request.ServiceName.ToLower()
                    && s.ServiceId != serviceId
                );
            if (duplicateService)
            {
                return new ResponseValue<ServiceDTO>(
                    null,
                    StatusReponse.BadRequest,
                    "Tên dịch vụ đã tồn tại."
                );
            }
            //cập nhật
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                service.ServiceName = request.ServiceName;
                service.ServiceType = request.ServiceType;
                service.Price = request.Price;
                service.Description = request.Description;
                await _serviceRepository.Update(service);

                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ResponseValue<ServiceDTO>(
                    new ServiceDTO
                    {
                        ServiceId = service.ServiceId,
                        ServiceName = service.ServiceName,
                        ServiceType = service.ServiceType,
                        Price = service.Price,
                        Description = service.Description,
                    },
                    StatusReponse.Success,
                    "Cập nhật dịch vụ thành công."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseValue<ServiceDTO>(
                    null,
                    StatusReponse.Error,
                    "Đã xảy ra lỗi khi cập nhật dịch vụ: " + ex.Message
                );
            }
        }
        catch (Exception ex)
        {
            return new ResponseValue<ServiceDTO>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi cập nhật dịch vụ: " + ex.Message
            );
        }
    }

    public async Task DeleteServiceAsync(int serviceId)
    {
        try
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null)
            {
                _logger.LogWarning("Không tìm thấy dịch vụ với serviceId: {ServiceId}", serviceId);
                throw new KeyNotFoundException("Service not found");
            }

            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                await _serviceRepository.DeleteAsync(serviceId);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xóa dịch vụ với serviceId: {ServiceId}", serviceId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa dịch vụ với serviceId: {ServiceId}", serviceId);
            throw;
        }
    }
}
