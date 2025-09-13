using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IPatientRepository : IRepository<Patient>
{
    // Add custom methods for Patient here if needed
}

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(SupabaseContext context)
        : base(context) { }
}
