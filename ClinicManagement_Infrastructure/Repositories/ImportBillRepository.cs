using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IImportBillRepository : IRepository<ImportBill>
{
    // Add custom methods for ImportBill here if needed
}

public class ImportBillRepository : Repository<ImportBill>, IImportBillRepository
{
    public ImportBillRepository(SupabaseContext context)
        : base(context) { }
}
