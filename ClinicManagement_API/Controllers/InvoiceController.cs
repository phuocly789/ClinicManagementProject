using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceController(IInvoiceService invoiceService, IInvoiceRepository invoiceRepository)
        {
            _invoiceService = invoiceService;
            _invoiceRepository = invoiceRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetHistory(
        [FromQuery] int? patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
            {
                var result = await _invoiceService.GetInvoiceHistoryAsync(patientId, page, pageSize);

                var totalItems = await _invoiceRepository.GetTotalCountAsync(patientId);

                var response = new
                {
                    success = result.Status == StatusReponse.Success,
                    data = new
                    {
                        totalItems,
                        page,
                        pageSize,
                        invoices = result.Content ?? new List<InvoiceDTO>()
                    },
                    message = result.Message
                };
            return Ok(response);
        }
    }
}