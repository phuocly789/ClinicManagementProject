using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IMedicalRecordRepository : IRepository<MedicalRecord>
{
    // Add custom methods for MedicalRecord here if needed
}

public class MedicalRecordRepository : Repository<MedicalRecord>, IMedicalRecordRepository
{
    public MedicalRecordRepository(SupabaseContext context)
        : base(context) { }
}
