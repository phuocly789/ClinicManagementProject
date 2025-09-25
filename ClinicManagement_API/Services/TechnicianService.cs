using Microsoft.EntityFrameworkCore;

public interface ITechnicianService
{
    Task<ResponseValue<ServiceAssignmentDTO>> GetAssignedServiceOrdersAsync(
        DateTime? date,
        int currentUserId
    );
    Task<ResponseValue<ServiceOrderResultDto>> UpdateServiceOrderResultAsync(
        int serviceOrderId,
        ServiceOrderUpdateDto updateDto,
        int currentUserId
    );
}

public class TechnicianService : ITechnicianService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public TechnicianService(
        IUnitOfWork unitOfWork,
        IServiceOrderRepository serviceOrderRepository,
        IAppointmentRepository appointmentRepository
    )
    {
        _unitOfWork = unitOfWork;
        _serviceOrderRepository = serviceOrderRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<ResponseValue<ServiceAssignmentDTO>> GetAssignedServiceOrdersAsync(
        DateTime? date,
        int currentUserId
    )
    {
        //lấy danh sách ServiceOrder được gán cho kỹ thuật viên hiện tại
        var serviceOrders = await _serviceOrderRepository.GetAssignedServiceOrdersAsync(
            currentUserId,
            date
        );
        if (serviceOrders == null || !serviceOrders.Any())
        {
            return new ResponseValue<ServiceAssignmentDTO>(
                null,
                StatusReponse.NotFound,
                "No assigned service orders found for the specified date."
            );
        }
        //lấy thông tin bệnh nhân và dịch vụ từ các bảng liên quan
        var appointmentIds = serviceOrders.Select(so => so.AppointmentId).Distinct().ToList();
        var appointments = await _appointmentRepository
            .GetAll()
            .Where(a => appointmentIds.Contains(a.AppointmentId))
            .ToListAsync();

        var patientIds = appointments.Select(a => a.PatientId).Distinct().ToList();
        var patients = await _serviceOrderRepository.GetUsersByIdsAsync(patientIds);

        var serviceIds = serviceOrders.Select(so => so.ServiceId).Distinct().ToList();
        var services = await _serviceOrderRepository.GetServicesByIdsAsync(serviceIds);

        //response
        var result = serviceOrders
            .Select(so => new ServiceAssignmentDto
            {
                ServiceOrderId = so.ServiceOrderId,
                AppointmentId = so.AppointmentId,
                PatientId = appointments
                    .FirstOrDefault(a => a.AppointmentId == so.AppointmentId)
                    ?.PatientId,
                PatientName = patients
                    .FirstOrDefault(p =>
                        p.UserId
                        == appointments
                            .FirstOrDefault(a => a.AppointmentId == so.AppointmentId)
                            ?.PatientId
                    )
                    ?.FullName,
                ServiceId = so.ServiceId,
                ServiceName = services
                    .FirstOrDefault(s => s.ServiceId == so.ServiceId)
                    ?.ServiceName,
                OrderDate = so.OrderDate,
                Status = so.Status,
            })
            .ToList();
        return new ResponseValue<ServiceAssignmentDTO>(
            new ServiceAssignmentDTO { ServiceAssignments = result },
            StatusReponse.Success,
            "Assigned service orders retrieved successfully."
        );
    }

    public async Task<ResponseValue<ServiceOrderResultDto>> UpdateServiceOrderResultAsync(
        int serviceOrderId,
        ServiceOrderUpdateDto updateDto,
        int currentUserId
    )
    {
        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(serviceOrderId);
        if (serviceOrder == null)
        {
            return new ResponseValue<ServiceOrderResultDto>(
                null,
                StatusReponse.NotFound,
                "Service order not found."
            );
        }
        if (serviceOrder.AssignedStaffId != currentUserId)
        {
            return new ResponseValue<ServiceOrderResultDto>(
                null,
                StatusReponse.Unauthorized,
                "You are not authorized to update this service order."
            );
        }
        if (updateDto.Status != "Completed" && updateDto.Status != "Cancelled")
        {
            return new ResponseValue<ServiceOrderResultDto>(
                null,
                StatusReponse.BadRequest,
                "Invalid status. Only 'Completed' or 'Cancelled' are allowed."
            );
        }
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        serviceOrder.Status = updateDto.Status;
        serviceOrder.Result = updateDto.Result;

        await _serviceOrderRepository.Update(serviceOrder);
        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();

        return new ResponseValue<ServiceOrderResultDto>(
            new ServiceOrderResultDto
            {
                ServiceOrderId = serviceOrder.ServiceOrderId,
                Status = serviceOrder.Status,
            },
            StatusReponse.Success,
            "Service order updated successfully."
        );
    }
}
