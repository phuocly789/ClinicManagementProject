using System;
using System.Collections.Generic;

namespace ClinicManagement_API.Models;

/// <summary>
/// Auth: Manages updates to the auth system.
/// </summary>
public partial class SchemaMigration
{
    public string Version { get; set; } = null!;
}
