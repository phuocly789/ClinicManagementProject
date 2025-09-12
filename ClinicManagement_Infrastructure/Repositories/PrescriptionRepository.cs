using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IPrescriptionRepository : IRepository<Prescription>
{
    // Add custom methods for Prescription here if needed
}

public class PrescriptionRepository : Repository<Prescription>, IPrescriptionRepository
{
    public PrescriptionRepository(SupabaseContext context)
        : base(context) { }
}
