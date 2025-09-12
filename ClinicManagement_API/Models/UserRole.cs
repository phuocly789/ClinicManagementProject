using System;
using System.Collections.Generic;

namespace ClinicManagement_API.Models;

public partial class UserRole
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User1 User { get; set; } = null!;
}
