using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IPrescriptionDetailRepository : IRepository<PrescriptionDetail>
{
    // Add custom methods for PrescriptionDetail here if needed
}

public class PrescriptionDetailRepository
    : Repository<PrescriptionDetail>,
        IPrescriptionDetailRepository
{
    public PrescriptionDetailRepository(SupabaseContext context)
        : base(context) { }
}
