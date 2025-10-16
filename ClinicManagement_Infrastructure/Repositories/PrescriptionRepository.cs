using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IPrescriptionRepository : IRepository<Prescription>
{
    // Add custom methods for Prescription here if needed
    Task<bool> IsValidAppointmentAsync(int appointmentId, int staffId);
    Task<bool> IsValidMedicalRecordAsync(int recordId, int appointmentId);
    Task<bool> HasEnoughStockAsync(int medicineId, int quantity);
    Task UpdateMedicineStockAsync(int medicineId, int quantity);
    Task<List<PrescriptionDetail>> GetPrescriptionDetailsAsync(int prescription);
    Task<List<Medicine>> GetMedicinesByIdsAsync(List<int?> medicineIds);
}

public class PrescriptionRepository : Repository<Prescription>, IPrescriptionRepository
{
    public PrescriptionRepository(SupabaseContext context)
        : base(context) { }

    public async Task<bool> IsValidAppointmentAsync(int appointmentId, int staffId)
    {
        return await _context.Appointments.AnyAsync(a =>
            a.AppointmentId == appointmentId && a.StaffId == staffId
        );
    }

    public async Task<bool> IsValidMedicalRecordAsync(int recordId, int appointmentId)
    {
        return await _context.MedicalRecords.AnyAsync(m =>
            m.RecordId == recordId && m.Appointments.Any(a => a.AppointmentId == appointmentId)
        );
    }

    public async Task<bool> HasEnoughStockAsync(int medicineId, int quantity)
    {
        var medicine = await _context.Medicines.FindAsync(medicineId);
        return medicine != null && medicine.StockQuantity >= quantity;
    }

    public async Task UpdateMedicineStockAsync(int medicineId, int quantity)
    {
        var medicine = await _context.Medicines.FindAsync(medicineId);
        if (medicine != null)
        {
            medicine.StockQuantity -= quantity;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<PrescriptionDetail>> GetPrescriptionDetailsAsync(int prescriptionId)
    {
        return await _context
            .PrescriptionDetails.Where(pd => pd.PrescriptionId == prescriptionId)
            .ToListAsync();
    }

    public async Task<List<Medicine>> GetMedicinesByIdsAsync(List<int?> medicineIds)
    {
        return await _context
            .Medicines.Where(m => medicineIds.Contains(m.MedicineId))
            .ToListAsync();
    }
}
