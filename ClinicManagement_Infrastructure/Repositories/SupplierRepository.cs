using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;

public interface ISupplierRepository : IRepository<Supplier>
{
    // Add custom methods for Supplier here if needed
}

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(SupabaseContext context)
        : base(context) { }
}
