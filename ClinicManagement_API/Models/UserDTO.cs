using System;
using System.Collections.Generic;

namespace ClinicManagement_API.Models;

/// <summary>
/// Auth: Stores user login data within a secure schema.
/// </summary>
public partial class UserDTO
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;
}
