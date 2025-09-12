using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IMedicalStaffRepository : IRepository<MedicalStaff>
{
    // Add custom methods for MedicalStaff here if needed
}

public class MedicalStaffRepository : Repository<MedicalStaff>, IMedicalStaffRepository
{
    public MedicalStaffRepository(SupabaseContext context)
        : base(context) { }
}
