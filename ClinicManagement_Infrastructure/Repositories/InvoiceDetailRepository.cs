using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Data.Models;

public interface IInvoiceDetailRepository : IRepository<InvoiceDetail>
{
    // Add custom methods for InvoiceDetail here if needed
}

public class InvoiceDetailRepository : Repository<InvoiceDetail>, IInvoiceDetailRepository
{
    public InvoiceDetailRepository(SupabaseContext context)
        : base(context) { }
}
