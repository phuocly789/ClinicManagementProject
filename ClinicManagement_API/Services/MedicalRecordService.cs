using ClinicManagement_Infrastructure.Data.Models;

public interface IMedicalRecordService
{
    Task<List<MedicalRecordDTO>> GetAllMedicalRecordsAsync();
    Task<ResponseValue<MedicalRecordDTO>> MedicalRecordCreateAsync(MedicalRecordDTO request, int createdBy);
}

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly ILogger<MedicalRecordService> _logger;
    private readonly IUnitOfWork _uow;

    public MedicalRecordService(
        IMedicalRecordRepository medicalRecordRepository,
        ILogger<MedicalRecordService> logger,
        IUnitOfWork uow
    )
    {
        _medicalRecordRepository = medicalRecordRepository;
        _logger = logger;
        _uow = uow;
    }

    public async Task<List<MedicalRecordDTO>> GetAllMedicalRecordsAsync()
    {
        try
        {
            var medicalRecords = await _medicalRecordRepository.GetAllAsync();

            var medicalRecordList = medicalRecords.Select(mr => new MedicalRecordDTO
            {
                RecordId = mr.RecordId,
                PatientId = mr.PatientId,
                RecordNumber = mr.RecordNumber,
                IssuedDate = mr.IssuedDate,
                Status = mr.Status,
                Notes = mr.Notes,
                CreatedBy = mr.CreatedBy
            }).ToList();

            return medicalRecordList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching medical record list");
            throw;
        }
    }

    public async Task<ResponseValue<MedicalRecordDTO>> MedicalRecordCreateAsync(MedicalRecordDTO request, int createdBy)
    {
        try
        {
            using var transaction = await _uow.BeginTransactionAsync();

            var medicalRecord = new MedicalRecord
            {
                PatientId = request.PatientId,
                RecordNumber = request.RecordNumber,
                IssuedDate = request.IssuedDate,
                Status = "Active",
                Notes = "Cấp lần đầu",
                CreatedBy = createdBy
            };

            await _medicalRecordRepository.AddAsync(medicalRecord);
            await _uow.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResponseValue<MedicalRecordDTO>(
                new MedicalRecordDTO
                {
                    RecordId = medicalRecord.RecordId,
                    PatientId = medicalRecord.PatientId,
                    RecordNumber = medicalRecord.RecordNumber,
                    IssuedDate = medicalRecord.IssuedDate,
                    Status = medicalRecord.Status,
                    Notes = medicalRecord.Notes,
                    CreatedBy = medicalRecord.CreatedBy
                },
                StatusReponse.Success,
                "Tạo hồ sơ bệnh án thành công."
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Lỗi xác thực khi thêm vào hồ sơ bệnh án: {@Request}", request);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Đã có lỗi xảy ra khi thêm vào hồ sơ bệnh án: {@Request}", request);
            throw;
        }
    }
}