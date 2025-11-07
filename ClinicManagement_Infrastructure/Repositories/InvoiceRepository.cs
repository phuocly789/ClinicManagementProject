using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IInvoiceRepository : IRepository<Invoice>
{
    // Add custom methods for Invoice here if needed
    IQueryable<Invoice> GetAllWithPatient();
    Task<int> GetTotalCountAsync(int? patientId = null);
}

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(SupabaseContext context)
        : base(context) { }

    public IQueryable<Invoice> GetAllWithPatient()
    {
        return _context.Invoices
            .Include(i => i.Patient)
            .AsNoTracking();
    }

    public async Task<int> GetTotalCountAsync(int? patientId = null)
    {
        var query = _context.Invoices
            .Include(i => i.Patient)
            .AsNoTracking();

        if (patientId.HasValue && patientId.Value > 0)
            query = query.Where(i => i.PatientId == patientId.Value);

        return await query.CountAsync();
    }
}
