public class SendOtpRequest
{
    public string Email { get; set; }
}

public class VerifyOtpRequest
{
    public string Email { get; set; }
    public string OTP { get; set; }
}

public class ResetPasswordRequest
{
    public string Email { get; set; }
    public string NewPassword { get; set; }
}
