using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IMedicalRecordRepository : IRepository<MedicalRecord>
{
    Task<MedicalRecord?> GetByPatientIdAsync(int patientId);
}

public class MedicalRecordRepository : Repository<MedicalRecord>, IMedicalRecordRepository
{
    public MedicalRecordRepository(SupabaseContext context)
        : base(context) { }

    public async Task<MedicalRecord?> GetByPatientIdAsync(int patientId)
    {
        return await _context.MedicalRecords
            .FirstOrDefaultAsync(r => r.PatientId == patientId);
    }
}
