using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IMedicineRepository : IRepository<Medicine>
{
    // Add custom methods for Medicine here if needed
}

public class MedicineRepository : Repository<Medicine>, IMedicineRepository
{
    public MedicineRepository(SupabaseContext context)
        : base(context) { }
}
