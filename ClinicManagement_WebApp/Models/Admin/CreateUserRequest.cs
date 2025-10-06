using System.ComponentModel.DataAnnotations;

public class CreateUserRequest
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    // [Required]
    // public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    public string? Phone { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Required]
    public int RoleId { get; set; }

    [MaxLength(20)]
    public string? StaffType { get; set; } // Nếu là nhân viên y tế

    [MaxLength(100)]
    public string? Specialty { get; set; } // Nếu là bác sĩ

    [MaxLength(50)]
    public string? LicenseNumber { get; set; } // Nếu là bác sĩ

    [MaxLength(500)]
    public string? Bio { get; set; }
}

public class CreateUserResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
