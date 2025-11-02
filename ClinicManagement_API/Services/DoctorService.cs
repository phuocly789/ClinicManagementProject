using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public interface IDoctorService
{
    Task<List<AppointmentMyScheduleDto>> GetAppointmentsByStaffIdAnddDateAsync(
        DateOnly? date = null,
        int staffId = 0
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
    Task<List<TodaysAppointmentDTO>> GetTodaysAppointmentsAsync(DateOnly date);

    // ----------------------------------------
    Task<ResponseValue<object>> SubmitExaminationAsync(ExaminationRequestDto request, int staffId);
    Task<ResponseValue<PagedResult<MedicineDTO>>> GetAllMedicinesAsync(string? search);
    Task<ResponseValue<PagedResult<ServiceDTO>>> GetAllServicesAsync(string? search);
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
    private readonly IMedicineRepository _medicineRepository;
    private readonly IServiceRepository _serviceRepository;



    public DoctorService(
        IAppointmentRepository appointmentRepository,
        IDiagnosisRepository diagnosisRepository,
        IUnitOfWork uow,
        IServiceService serviceService,
        IUserRepository userRepository,
        IServiceOrderRepository serviceOrderRepository,
        IMedicalStaffRepository medicalStaffRepository,
        IPrescriptionRepository prescriptionRepository,
        IPrescriptionDetailRepository prescriptionDetailRepository,
        IMedicineRepository medicineRepository,
        IServiceRepository serviceRepository
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
        _medicineRepository = medicineRepository;
        _serviceRepository = serviceRepository;
    }

    //lấy lịch làm
    public async Task<List<AppointmentMyScheduleDto>> GetAppointmentsByStaffIdAnddDateAsync(
        DateOnly? date = null,
        int staffId = 0
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

    //for dashboard
    public async Task<List<TodaysAppointmentDTO>> GetTodaysAppointmentsAsync(DateOnly date)
    {
        return await _appointmentRepository
            .GetAll()
            .Where(a => a.AppointmentDate == date)
            .Include(a => a.Patient)
            .Include(a => a.Staff)
            .OrderBy(a => a.AppointmentTime)
            .Select(a => new TodaysAppointmentDTO
            {
                AppointmentId = a.AppointmentId,
                AppointmentTime = a.AppointmentTime,
                PatientName = a.Patient.FullName,
                DoctorName = a.Staff.FullName,
                Status = a.Status,
            })
            .ToListAsync();
    }

    public async Task<ResponseValue<object>> SubmitExaminationAsync(
        ExaminationRequestDto request,
        int staffId
    )
    {
        // Bắt đầu một transaction để đảm bảo tất cả các thao tác đều thành công hoặc thất bại cùng nhau
        using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            // 1. Tìm cuộc hẹn và kiểm tra quyền của bác sĩ
            var appointment = await _appointmentRepository
                .GetAll()
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == request.AppointmentId && a.StaffId == staffId
                );

            if (appointment == null)
            {
                return new ResponseValue<object>(
                    null,
                    StatusReponse.NotFound,
                    "Không tìm thấy cuộc hẹn hợp lệ cho bác sĩ này."
                );
            }

            // 2. Xử lý Chẩn đoán (Lưu hoặc Cập nhật)
            var diagnosis = await _diagnosisRepository
                .GetAll()
                .FirstOrDefaultAsync(d => d.AppointmentId == request.AppointmentId);

            if (diagnosis == null) // Nếu chưa có chẩn đoán -> tạo mới
            {
                diagnosis = new Diagnosis
                {
                    AppointmentId = request.AppointmentId,
                    StaffId = staffId,
                    RecordId = appointment.RecordId,
                    DiagnosisDate = DateTime.UtcNow, // Sử dụng UTC để nhất quán
                };
                await _diagnosisRepository.AddAsync(diagnosis);
            }
            // Cập nhật thông tin chẩn đoán từ request
            diagnosis.Symptoms = request.Symptoms;
            diagnosis.Diagnosis1 = request.Diagnosis;
            // Lưu thay đổi để diagnosis có ID nếu là bản ghi mới
            await _uow.SaveChangesAsync();

            // 3. Xử lý Đơn thuốc (Prescriptions)
            // Tìm đơn thuốc đã có của cuộc hẹn này
            var prescription = await _prescriptionRepository
                .GetAll()
                .FirstOrDefaultAsync(p => p.AppointmentId == request.AppointmentId);

            // Nếu có đơn thuốc cũ, xóa hết chi tiết cũ để thêm lại danh sách mới
            if (prescription != null)
            {
                var oldDetails = await _prescriptionDetailRepository
                    .GetAll()
                    .Where(pd => pd.PrescriptionId == prescription.PrescriptionId)
                    .ToListAsync();

                if (oldDetails.Any())
                {
                    _prescriptionDetailRepository.RemoveRange(oldDetails);
                    await _uow.SaveChangesAsync();
                }
            }

            // Nếu frontend gửi lên danh sách thuốc mới
            if (request.Prescriptions != null && request.Prescriptions.Any())
            {
                // Nếu chưa có đơn thuốc, tạo mới
                if (prescription == null)
                {
                    prescription = new Prescription
                    {
                        AppointmentId = request.AppointmentId,
                        RecordId = appointment.RecordId,
                        PrescriptionDate = DateTime.UtcNow,
                        StaffId = staffId,
                        Instructions = "Uống theo chỉ dẫn của bác sĩ.", // Có thể thêm trường này vào DTO nếu cần
                    };
                    await _prescriptionRepository.AddAsync(prescription);
                    await _uow.SaveChangesAsync(); // Lưu để lấy PrescriptionId
                }

                // Thêm các chi tiết đơn thuốc mới
                foreach (var detailDto in request.Prescriptions)
                {
                    // KIỂM TRA TỒN KHO TRƯỚC KHI THÊM
                    bool hasStock = await _prescriptionRepository.HasEnoughStockAsync(
                        detailDto.MedicineId,
                        detailDto.Quantity
                    );
                    if (!hasStock)
                    {
                        // Nếu không đủ thuốc, hủy toàn bộ transaction và báo lỗi
                        throw new Exception(
                            $"Không đủ số lượng tồn kho cho thuốc ID {detailDto.MedicineId}"
                        );
                    }

                    var detail = new PrescriptionDetail
                    {
                        PrescriptionId = prescription.PrescriptionId,
                        MedicineId = detailDto.MedicineId,
                        Quantity = detailDto.Quantity,
                        DosageInstruction = detailDto.DosageInstruction,
                    };
                    await _prescriptionDetailRepository.AddAsync(detail);

                    // TRỪ TỒN KHO
                    await _prescriptionRepository.UpdateMedicineStockAsync(
                        detailDto.MedicineId,
                        detailDto.Quantity
                    );
                }
            }

            // 4. Xử lý Dịch vụ được chỉ định (Service Orders)
            // Xóa tất cả các dịch vụ đã chỉ định trước đó cho cuộc hẹn này để đồng bộ lại
            var oldServiceOrders = await _serviceOrderRepository
                .GetAll()
                .Where(so => so.AppointmentId == request.AppointmentId)
                .ToListAsync();

            if (oldServiceOrders.Any())
            {
                _serviceOrderRepository.RemoveRange(oldServiceOrders);
            }

            // Tạo lại các dịch vụ mới từ request
            if (request.ServiceIds != null && request.ServiceIds.Any())
            {
                foreach (var serviceId in request.ServiceIds)
                {
                    var serviceOrder = new ServiceOrder
                    {
                        AppointmentId = request.AppointmentId,
                        ServiceId = serviceId,
                        OrderDate = DateTime.UtcNow,
                        Status = "Pending", // Trạng thái chờ thực hiện
                        // AssignedStaffId có thể null hoặc cần logic để gán cho kỹ thuật viên
                    };
                    await _serviceOrderRepository.AddAsync(serviceOrder);
                }
            }

            // 5. Cập nhật trạng thái cuộc hẹn nếu bác sĩ chọn "Hoàn tất"
            if (request.IsComplete)
            {
                // appointment.Status = "Completed";
                var queue = await _uow.GetDbContext()
                    .Queues.FirstOrDefaultAsync(q => q.AppointmentId == request.AppointmentId);
                if (queue != null)
                {
                    queue.Status = "Completed";
                    appointment.Status = "Đã khám";
                }
            }

            // Lưu tất cả thay đổi vào database
            await _uow.SaveChangesAsync();
            // Hoàn tất transaction
            await transaction.CommitAsync();

            return new ResponseValue<object>(
                null,
                StatusReponse.Success,
                request.IsComplete ? "Hoàn tất khám bệnh thành công" : "Tạm lưu thành công"
            );
        }
        catch (Exception ex)
        {
            // Nếu có bất kỳ lỗi nào, hủy bỏ tất cả các thay đổi
            await transaction.RollbackAsync();
            return new ResponseValue<object>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi hệ thống: " + ex.Message
            );
        }
    }

    public async Task<ResponseValue<PagedResult<MedicineDTO>>> GetAllMedicinesAsync(
        string? search = null
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

            //get total count
            var totalItems = await query.CountAsync();

            //fetch services with pagination
            var medicines = await query
                .OrderBy(m => m.MedicineId)
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
                    Page = 0,
                    PageSize = 0,
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
     public async Task<ResponseValue<PagedResult<ServiceDTO>>> GetAllServicesAsync(
        string? search
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
                    Page = 0,
                    PageSize = 0,
                    Items = services,
                },
                StatusReponse.Success,
                "Lấy danh sách dịch vụ thành công."
            );
        }
        catch (Exception ex)
        {
            return new ResponseValue<PagedResult<ServiceDTO>>(
                null,
                StatusReponse.Error,
                "Đã xảy ra lỗi khi lấy danh sách dịch vụ: " + ex.Message
            );
        }
    }
}
