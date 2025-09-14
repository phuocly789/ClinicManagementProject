using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Models;

public partial class OauthClient
{
    public Guid Id { get; set; }

    public string ClientId { get; set; } = null!;

    public string ClientSecretHash { get; set; } = null!;

    public string RedirectUris { get; set; } = null!;

    public string GrantTypes { get; set; } = null!;

    public string? ClientName { get; set; }

    public string? ClientUri { get; set; }

    public string? LogoUri { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
