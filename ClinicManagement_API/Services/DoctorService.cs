using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public interface IDoctorService
{
    Task<List<AppointmentMyScheduleDto>> GetAppointmentsByStaffIdAnddDateAsync(
        int staffId,
        DateOnly? date = null
    );
    Task<DiagnosisDataDto> CreateDiagnosisAsync(CreateDiagnosisDto request, int currentStaffId);
    Task<ResponseValue<ServiceOrderResponseDto>> CreateServiceOrderAsync(
        CreateServiceOrderDto request,
        int currentStaffId
    );
    Task<ResponseValue<PrescriptionResponseDto>> CreatePrescriptionAsync(
        PrescriptionRequestDto request,
        int currentStaffId
    );
    Task<ResponseValue<ServiceOrderResultDto>> GetAppointmentResultsAsync(
        int appointmentId,
        int currentStaffId
    );
}

public class DoctorService : IDoctorService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDiagnosisRepository _diagnosisRepository;
    private readonly IUnitOfWork _uow;
    private readonly IServiceService _serviceService;
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMedicalStaffRepository _medicalStaffRepository;
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IPrescriptionDetailRepository _prescriptionDetailRepository;

    public DoctorService(
        IAppointmentRepository appointmentRepository,
        IDiagnosisRepository diagnosisRepository,
        IUnitOfWork uow,
        IServiceService serviceService,
        IUserRepository userRepository,
        IServiceOrderRepository serviceOrderRepository,
        IMedicalStaffRepository medicalStaffRepository,
        IPrescriptionRepository prescriptionRepository,
        IPrescriptionDetailRepository prescriptionDetailRepository
    )
    {
        _appointmentRepository = appointmentRepository;
        _diagnosisRepository = diagnosisRepository;
        _serviceService = serviceService;
        _userRepository = userRepository;
        _serviceOrderRepository = serviceOrderRepository;
        _uow = uow;
        _medicalStaffRepository = medicalStaffRepository;
        _prescriptionRepository = prescriptionRepository;
        _prescriptionDetailRepository = prescriptionDetailRepository;
    }

    //lấy lịch làm
    public async Task<List<AppointmentMyScheduleDto>> GetAppointmentsByStaffIdAnddDateAsync(
        int staffId,
        DateOnly? date = null
    )
    {
        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Now);

        return await _appointmentRepository.GetAppointmentsByStaffIdAnddDateAsync(
            staffId,
            selectedDate
        );
    }

    //tạo chuẩn đoán
    public async Task<DiagnosisDataDto> CreateDiagnosisAsync(
        CreateDiagnosisDto request,
        int currentStaffId
    )
    {
        //kiểm tra đầu vào
        if (
            string.IsNullOrWhiteSpace(request.Symptoms)
            || string.IsNullOrWhiteSpace(request.Diagnosis)
        )
        {
            return null;
        }

        //kiểm tra AppointmentId và staffId
        if (
            !await _diagnosisRepository
                .GetAll()
                .AsNoTracking()
                .AnyAsync(d =>
                    d.AppointmentId == request.AppointmentId && d.StaffId == currentStaffId
                )
        )
        {
            return null;
        }

        //kiểm tra recordId
        if (
            !await _appointmentRepository
                .GetAll()
                .AsNoTracking()
                .AnyAsync(a => a.RecordId == request.RecordId)
        )
        {
            return null;
        }
        using var transaction = await _uow.BeginTransactionAsync();
        //tạo bảng ghi chuẩn đoán
        var diagnosis = new Diagnosis
        {
            AppointmentId = request.AppointmentId,
            StaffId = currentStaffId,
            RecordId = request.RecordId,
            Symptoms = request.Symptoms,
            Diagnosis1 = request.Diagnosis,
            Notes = request.Notes,
            DiagnosisDate = DateTime.Now,
        };
        await _diagnosisRepository.AddAsync(diagnosis);
        await _uow.SaveChangesAsync();
        await transaction.CommitAsync();

        return new DiagnosisDataDto
        {
            DiagnosisId = diagnosis.DiagnosisId,
            AppointmentId = diagnosis.AppointmentId,
            StaffId = diagnosis.StaffId,
            Symptoms = diagnosis.Symptoms,
            Diagnosis = diagnosis.Diagnosis1,
            DiagnosisDate = diagnosis.DiagnosisDate,
        };
    }

    //chỉ định sử dụng dịch vụ
    public async Task<ResponseValue<ServiceOrderResponseDto>> CreateServiceOrderAsync(
        CreateServiceOrderDto request,
        int currentStaffId
    )
    {
        //ktra appointment
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);
        if (appointment == null || appointment.StaffId != currentStaffId)
        {
            return new ResponseValue<ServiceOrderResponseDto>(
                null,
                StatusReponse.NotFound,
                "Appointment not found"
            );
        }

        //kiểm tra service tồn tại
        var service = await _serviceService.GetServiceByIdAsync(request.ServiceId);
        if (service == null)
        {
            return new ResponseValue<ServiceOrderResponseDto>(
                null,
                StatusReponse.NotFound,
                "Service not found"
            );
        }

        //kiểm tra người dùng tồn tại là kĩ thuật viên
        var staff = await _userRepository.GetByIdAsync(request.AssignedStaffId);
        if (staff == null)
        {
            return new ResponseValue<ServiceOrderResponseDto>(
                null,
                StatusReponse.NotFound,
                "Staff not found"
            );
        }

        var medicalStaff = await _medicalStaffRepository
            .GetAll()
            .FirstOrDefaultAsync(ms => ms.StaffId == request.AssignedStaffId);
        if (medicalStaff == null || medicalStaff.StaffType != "Technician")
        {
            return new ResponseValue<ServiceOrderResponseDto>(
                null,
                StatusReponse.BadRequest,
                "Assigned staff is not a technician"
            );
        }

        //tạo service mới
        using var transaction = await _uow.BeginTransactionAsync();
        var serviceOrder = new ServiceOrder
        {
            AppointmentId = request.AppointmentId,
            ServiceId = request.ServiceId,
            AssignedStaffId = request.AssignedStaffId,
            OrderDate = DateTime.Now,
            Status = "Pending",
        };
        await _serviceOrderRepository.AddAsync(serviceOrder);
        await _uow.SaveChangesAsync();
        await transaction.CommitAsync();

        return new ResponseValue<ServiceOrderResponseDto>(
            new ServiceOrderResponseDto
            {
                ServiceOrderId = serviceOrder.ServiceOrderId,
                AppointmentId = serviceOrder.AppointmentId,
                ServiceId = serviceOrder.ServiceId,
                AssignedStaffId = serviceOrder.AssignedStaffId,
                OrderDate = serviceOrder.OrderDate,
                Status = serviceOrder.Status,
            },
            StatusReponse.Success,
            "Tạo đơn dịch vụ thành công"
        );
    }

    //kê toa thuốc
    public async Task<ResponseValue<PrescriptionResponseDto>> CreatePrescriptionAsync(
        PrescriptionRequestDto request,
        int currentStaffId
    )
    {
        // Validate input
        if (request == null || request.Details == null || !request.Details.Any())
        {
            return new ResponseValue<PrescriptionResponseDto>(
                null,
                StatusReponse.BadRequest,
                "Invalid prescription request"
            );
        }

        // Validate appointment and staff
        var isValidAppointment = await _prescriptionRepository.IsValidAppointmentAsync(
            request.AppointmentId,
            currentStaffId
        );
        if (!isValidAppointment)
        {
            return new ResponseValue<PrescriptionResponseDto>(
                null,
                StatusReponse.BadRequest,
                "Invalid appointment or staff"
            );
        }

        // Validate medical record
        bool isValidRecord = await _prescriptionRepository.IsValidMedicalRecordAsync(
            request.RecordId,
            request.AppointmentId
        );
        if (!isValidRecord)
        {
            return new ResponseValue<PrescriptionResponseDto>(
                null,
                StatusReponse.BadRequest,
                "Invalid medical record for the appointment"
            );
        }

        // Validate medicine stock
        foreach (var detail in request.Details)
        {
            bool hasStock = await _prescriptionRepository.HasEnoughStockAsync(
                detail.MedicineId,
                detail.Quantity
            );
            if (!hasStock)
            {
                return new ResponseValue<PrescriptionResponseDto>(
                    null,
                    StatusReponse.BadRequest,
                    $"Not enough stock for medicine ID {detail.MedicineId}"
                );
            }
        }

        using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            // Create prescription
            var prescription = new Prescription
            {
                AppointmentId = request.AppointmentId,
                RecordId = request.RecordId,
                PrescriptionDate = DateTime.Now,
                Instructions = request.Instructions,
                StaffId = currentStaffId,
            };
            await _prescriptionRepository.AddAsync(prescription);
            await _uow.SaveChangesAsync();

            // Create prescription details and update stock
            foreach (var detail in request.Details)
            {
                var prescriptionDetail = new PrescriptionDetail
                {
                    PrescriptionId = prescription.PrescriptionId,
                    MedicineId = detail.MedicineId,
                    Quantity = detail.Quantity,
                    DosageInstruction = detail.DosageInstruction,
                };
                await _prescriptionDetailRepository.AddAsync(prescriptionDetail);
                await _prescriptionRepository.UpdateMedicineStockAsync(
                    detail.MedicineId,
                    detail.Quantity
                );
            }
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();

            var prescriptionDetails = await _prescriptionRepository.GetPrescriptionDetailsAsync(
                prescription.PrescriptionId
            );
            var medicineIds = prescriptionDetails.Select(pd => pd.MedicineId).ToList();
            var medicines = await _prescriptionRepository.GetMedicinesByIdsAsync(medicineIds);
            return new ResponseValue<PrescriptionResponseDto>(
                new PrescriptionResponseDto
                {
                    PrescriptionId = prescription.PrescriptionId,
                    AppointmentId = prescription.AppointmentId,
                    PrescriptionDate = prescription.PrescriptionDate,
                    Instructions = prescription.Instructions,
                    Details = request
                        .Details.Select(d => new PrescriptionDetailDataDto
                        {
                            MedicineId = d.MedicineId,
                            Quantity = d.Quantity,
                            DosageInstruction = d.DosageInstruction,
                            MedicineName = medicines
                                .FirstOrDefault(m => m.MedicineId == d.MedicineId)
                                ?.MedicineName, // You can fetch medicine name if needed
                        })
                        .ToList(),
                },
                StatusReponse.Success,
                "Prescription created successfully"
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            return new ResponseValue<PrescriptionResponseDto>(
                null,
                StatusReponse.Error,
                "An error occurred while creating the prescription"
            );
        }
    }

    public async Task<ResponseValue<ServiceOrderResultDto>> GetAppointmentResultsAsync(
        int appointmentId,
        int currentStaffId
    )
    {
        // Validate appointment and staff
        var isValidAppointment = await _serviceOrderRepository.IsValidAppointmentAsync(
            appointmentId,
            currentStaffId
        );
        if (!isValidAppointment)
        {
            return new ResponseValue<ServiceOrderResultDto>(
                null,
                StatusReponse.BadRequest,
                "Invalid appointment or staff"
            );
        }

        // Fetch completed service orders for the appointment
        var completedServiceOrders = await _serviceOrderRepository.GetCompletedServiceOrdersAsync(
            appointmentId
        );
        if (completedServiceOrders == null || !completedServiceOrders.Any())
        {
            return new ResponseValue<ServiceOrderResultDto>(
                null,
                StatusReponse.NotFound,
                "No completed service orders found for the appointment"
            );
        }

        // Fetch service details for the completed service orders
        var serviceIds = completedServiceOrders.Select(so => so.ServiceId).ToList();
        var services = await _serviceOrderRepository.GetServicesByIdsAsync(serviceIds);
        var serviceDict = services.ToDictionary(s => s.ServiceId, s => s.ServiceName);

        // Map to result DTOs
        var results = completedServiceOrders
            .Select(so => new ServiceOrderResultDto
            {
                ServiceOrderId = so.ServiceOrderId,
                ServiceId = so.ServiceId,
                ServiceName = services
                    .FirstOrDefault(s => s.ServiceId == so.ServiceId)
                    ?.ServiceName,
                AssignedStaffId = so.AssignedStaffId,
                Result = so.Result,
                Status = so.Status,
                OrderDate = so.OrderDate,
            })
            .ToList();

        return new ResponseValue<ServiceOrderResultDto>(
            results.FirstOrDefault(), // Return the first result or modify as needed
            StatusReponse.Success,
            "Fetched completed service orders successfully"
        );
    }
}
