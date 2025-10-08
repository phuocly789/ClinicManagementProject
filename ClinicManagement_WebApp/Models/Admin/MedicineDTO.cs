using System.ComponentModel.DataAnnotations;

public class MedicineDTO
{
    [Required(ErrorMessage = "Mã thuốc là bắt buộc")]
    public int MedicineId { get; set; }

    [Required(ErrorMessage = "Tên thuốc là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên thuốc không được vượt quá 100 ký tự")]
    public string MedicineName { get; set; } = "";

    [Required(ErrorMessage = "Loại thuốc là bắt buộc")]
    [StringLength(50, ErrorMessage = "Loại thuốc không được vượt quá 50 ký tự")]
    public string MedicineType { get; set; } = "";

    [Required(ErrorMessage = "Đơn vị là bắt buộc")]
    [StringLength(20, ErrorMessage = "Đơn vị không được vượt quá 20 ký tự")]
    public string Unit { get; set; } = "";

    [Required(ErrorMessage = "Giá bán là bắt buộc")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Tồn kho là bắt buộc")]
    [Range(0, int.MaxValue, ErrorMessage = "Tồn kho phải lớn hơn hoặc bằng 0")]
    public int StockQuantity { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
    public string? Description { get; set; } = "";
}
