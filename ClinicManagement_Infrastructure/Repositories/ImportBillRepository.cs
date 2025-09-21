using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IImportBillRepository : IRepository<ImportBill>
{
    // Add custom methods for ImportBill here if needed
    Task<ImportBill?> GetByIdWithDetailsAsync(int id);
}

public class ImportBillRepository : Repository<ImportBill>, IImportBillRepository
{
    public ImportBillRepository(SupabaseContext context)
        : base(context) { }

    public async Task<ImportBill?> GetByIdWithDetailsAsync(int id)
    {
        var importBill = await GetByIdAsync(id);
        if (importBill == null)
            return null;

        // Lấy ImportDetails liên quan
        var importDetails = await _context
            .ImportDetails.Where(id => id.ImportId == importBill.ImportId)
            .ToListAsync();

        // Lấy Medicine cho từng ImportDetail
        var medicineIds = importDetails.Select(d => d.MedicineId).ToList();
        var medicines = await _context
            .Medicines.Where(m => medicineIds.Contains(m.MedicineId))
            .ToDictionaryAsync(m => m.MedicineId, m => m);

        importBill.ImportDetails = importDetails
            .Select(d => new ImportDetail
            {
                ImportDetailId = d.ImportDetailId,
                ImportId = d.ImportId,
                MedicineId = d.MedicineId,
                Quantity = d.Quantity,
                ImportPrice = d.ImportPrice,
                Medicine =
                    d.MedicineId.HasValue && medicines.ContainsKey(d.MedicineId.Value)
                        ? medicines[d.MedicineId.Value]
                        : null,
            })
            .ToList();

        return importBill;
    }
}
