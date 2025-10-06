public class UserDTO
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Gender { get; set; }
    public string Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public bool? IsActive { get; set; }
    public List<string> Roles { get; set; }
    public string Specialty { get; set; }
    public string LicenseNumber { get; set; }
}
