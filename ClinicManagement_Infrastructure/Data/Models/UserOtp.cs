using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Data.Models;

public partial class UserOtp
{
    public int Otpid { get; set; }

    public int? UserId { get; set; }

    public string Otpcode { get; set; } = null!;

    public DateTime ExpiredAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
