using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class Partner
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Website { get; set; }

    public string? Notes { get; set; }

    public bool IsPublic { get; set; }

    public string Status { get; set; } = null!;

    public Guid? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? LogoUrl { get; set; }

    public string? LogoPublicId { get; set; }

    public string? LogoFileType { get; set; }

    public long? LogoFileSize { get; set; }
}
