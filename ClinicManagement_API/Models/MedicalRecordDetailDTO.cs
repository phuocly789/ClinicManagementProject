using System.ComponentModel.DataAnnotations;

namespace ClinicManagement_API.Models;

public partial class MedicalRecordDetailDTO
{
    public int? RecordId { get; set; }

    public string? RecordNumber { get; set; }

    public DateOnly? IssuedDate { get; set; }

    public int? PatientId { get; set; }

    public string? PatientName { get; set; }

    public string? RecordStatus { get; set; }

    public List<AppointmentForPatientDTO> Appointments { get; set; } = new();
}

public class AppointmentForPatientDTO
{
    public int? AppointmentId { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public TimeOnly? AppointmentTime { get; set; }

    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorSpecialty { get; set; }

    // Các thông tin con
    public DiagnosisForPatientDTO? Diagnosis { get; set; }
    public PrescriptionForPatientDTO? Prescription { get; set; }
    public List<ServiceForPatientDTO> Services { get; set; } = new();
    public InvoiceForPatientDTO? Invoice { get; set; }
}

// ==================== DIAGNOSIS ====================
public class DiagnosisForPatientDTO
{
    public int? DiagnosisId { get; set; }
    public string? Symptoms { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public DateOnly? DiagnosisDate { get; set; }
}

// ==================== PRESCRIPTION ====================
public class PrescriptionForPatientDTO
{
    public int? PrescriptionId { get; set; }
    public DateTime? PrescriptionDate { get; set; }
    public string? Instructions { get; set; }
    public List<PrescriptionDetailForPatientDTO> Details { get; set; } = new();
}

public class PrescriptionDetailForPatientDTO
{
    public int PrescriptionDetailId { get; set; }
    public int MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public string? MedicineType { get; set; }
    public string? Unit { get; set; }
    public int Quantity { get; set; }
    public string? DosageInstruction { get; set; }
    public decimal Price { get; set; }
}

// ==================== SERVICE ====================
public class ServiceForPatientDTO
{
    public int? ServiceOrderId { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string? ServiceType { get; set; }
    public decimal? Price { get; set; }
    public string? Result { get; set; }
    public string? Status { get; set; }
    public DateTime? OrderDate { get; set; }
}

// ==================== INVOICE ====================
public class InvoiceForPatientDTO
{
    public int? InvoiceId { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? Status { get; set; }
    public List<InvoiceDetailForPatientDTO> Details { get; set; } = new();
}

public class InvoiceDetailForPatientDTO
{
    public int? InvoiceDetailId { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? SubTotal { get; set; }
}
