// Custom Validation Attribute cho tuổi tối thiểu
using System.ComponentModel.DataAnnotations;

public class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _minimumAge;

    public MinimumAgeAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"Người dùng phải ít nhất {_minimumAge} tuổi.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateOnly date)
        {
            var currentDate = new DateOnly(2025, 10, 7); // Ngày hiện tại theo yêu cầu
            var age = currentDate.Year - date.Year;
            if (date > currentDate.AddYears(-age))
                age--;
            if (age < _minimumAge)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
        }
        return ValidationResult.Success;
    }
}

public class CreateUserRequest
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [MaxLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [MaxLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [MaxLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
    [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Giới tính là bắt buộc")]
    [MaxLength(10, ErrorMessage = "Giới tính không được vượt quá 10 ký tự")]
    public string? Gender { get; set; }

    [MaxLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    [MinimumAge(18)]
    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Vai trò là bắt buộc")]
    [Range(2, 5, ErrorMessage = "Vui lòng chọn vai trò hợp lệ")]
    public int RoleId { get; set; }

    [MaxLength(20, ErrorMessage = "Loại nhân viên không được vượt quá 20 ký tự")]
    public string? StaffType { get; set; }

    [MaxLength(100, ErrorMessage = "Chuyên khoa không được vượt quá 100 ký tự")]
    public string? Specialty { get; set; }

    [MaxLength(50, ErrorMessage = "Số giấy phép không được vượt quá 50 ký tự")]
    public string? LicenseNumber { get; set; }

    [MaxLength(500, ErrorMessage = "Tiểu sử không được vượt quá 500 ký tự")]
    public string? Bio { get; set; }
}


