// "userId": 5,
//         "username": "letan03",
//         "fullName": "Hoàng Kim Chi",
//         "email": "letan03@clinic.com",
//         "phone": "0912345673",
//         "gender": "Nữ",
//         "address": "333 Sảnh Chờ, Q.Bình Thạnh, TP.HCM",
//         "dateOfBirth": "1998-11-30",
//         "isActive": true,
//         "roles": [
//           "Receptionist"
//         ],
//         "specialty": null,
//         "licenseNumber": null
public class UserVM
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    public bool? IsActive { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public string? Specialty { get; set; } // Nếu là bác sĩ
    public string? LicenseNumber { get; set; } // Nếu là bác sĩ
    public string? StaffType { get; set; }
    public string? Bio { get; set; }
}

