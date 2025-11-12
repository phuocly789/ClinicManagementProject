using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;

public interface IMedicalRecordDetailRepository : IRepository<MedicalRecordDetail>
{
    // Add custom methods for Patient here if needed
}

public class MedicalRecordDetailRepository
    : Repository<MedicalRecordDetail>,
        IMedicalRecordDetailRepository
{
    public MedicalRecordDetailRepository(SupabaseContext context)
        : base(context) { }
}
