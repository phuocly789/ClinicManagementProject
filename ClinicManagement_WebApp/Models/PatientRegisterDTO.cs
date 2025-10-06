using System.ComponentModel.DataAnnotations;

namespace ClinicManagement_API.Models;

public class PatientRegisterDto
{
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = null!;
}

public enum Gender
{
    // Giá trị 0
    Nam,

    // Giá trị 1
    Nữ,

    // Giá trị 2
    Khác,
}
