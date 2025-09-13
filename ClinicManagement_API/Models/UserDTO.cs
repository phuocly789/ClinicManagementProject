namespace ClinicManagement_API.Models;
public partial class UserDTO
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }
}
