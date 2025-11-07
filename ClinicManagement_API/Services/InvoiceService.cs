using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IInvoiceService
{
    Task<ResponseValue<List<InvoiceDTO>>> GetInvoiceHistoryAsync(
        int? patientId = null,
        int page = 1,
        int pageSize = 10
    );
}

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IInvoiceRepository invoiceRepo, ILogger<InvoiceService> logger)
    {
        _invoiceRepo = invoiceRepo;
        _logger = logger;
    }

    public async Task<ResponseValue<List<InvoiceDTO>>> GetInvoiceHistoryAsync(
    int? patientId = null,
    int page = 1,
    int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _invoiceRepo.GetAllWithPatient();

            if (patientId.HasValue && patientId.Value > 0)
                query = query.Where(i => i.PatientId == patientId.Value);

            var invoices = await query
                .OrderByDescending(i => i.InvoiceDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new InvoiceDTO
                {
                    InvoiceId = i.InvoiceId,
                    PatientId = i.PatientId ?? 0,
                    PatientName = i.Patient != null ? i.Patient.FullName : "Không xác định",
                    TotalAmount = i.TotalAmount,
                    InvoiceDate = i.InvoiceDate ?? DateTime.MinValue,
                    Status = i.Status
                })
                .ToListAsync();

            var message = invoices.Any()
                ? MessageResponse.Success
                : "Bệnh nhân chưa có hóa đơn.";

            return new ResponseValue<List<InvoiceDTO>>(
                content: invoices,
                status: StatusReponse.Success,
                message: message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi lấy lịch sử thanh toán. PatientId: {PatientId}", patientId);
            return new ResponseValue<List<InvoiceDTO>>(
                content: null,
                status: StatusReponse.Error,
                message: MessageResponse.Error
            );
        }
    }
}