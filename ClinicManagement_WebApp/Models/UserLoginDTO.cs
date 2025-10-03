using ClinicManagement_WebApp.Models;

public class UserLoginDTO
{
    public string? EmailOrPhone { get; set; }
    public string? Password { get; set; }
    public string Role { get; set; }
}
