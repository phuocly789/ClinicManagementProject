using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public interface IImportDetailRepository : IRepository<ImportDetail>
{
    // Add custom methods for ImportDetail here if needed
}

public class ImportDetailRepository : Repository<ImportDetail>, IImportDetailRepository
{
    public ImportDetailRepository(SupabaseContext context)
        : base(context) { }
}
