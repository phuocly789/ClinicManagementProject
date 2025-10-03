using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IDiagnosisRepository : IRepository<Diagnosis>
{
    // Add custom methods for Diagnosis here if needed
}

public class DiagnosisRepository : Repository<Diagnosis>, IDiagnosisRepository
{
    public DiagnosisRepository(SupabaseContext context)
        : base(context) { }
}
