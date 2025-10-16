using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IInvoiceRepository : IRepository<Invoice>
{
    // Add custom methods for Invoice here if needed
}

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(SupabaseContext context)
        : base(context) { }

   
}
