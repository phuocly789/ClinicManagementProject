using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;
public interface IPatientHistoryRepository : IRepository<PatientHistory>
{
    // Add custom methods for PatientHistory here if needed
}

public class PatientHistoryRepository : Repository<PatientHistory>, IPatientHistoryRepository
{
    public PatientHistoryRepository(SupabaseContext context)
        : base(context) { }
}
