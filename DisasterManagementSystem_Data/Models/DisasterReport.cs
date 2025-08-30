using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class DisasterReport
{
    public int Id { get; set; }

    public int? DisasterEventId { get; set; }

    public Guid? UserId { get; set; }

    public int LocationId { get; set; }

    public string? AddressDetail { get; set; }

    public string Type { get; set; } = null!;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Severity { get; set; }

    public string Status { get; set; } = null!;

    public string? Source { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AssistanceRequest> AssistanceRequests { get; set; } = new List<AssistanceRequest>();

    public virtual DisasterEvent? DisasterEvent { get; set; }

    public virtual ICollection<Impact> Impacts { get; set; } = new List<Impact>();

    public virtual Location Location { get; set; } = null!;

    public virtual ICollection<ReportPhoto> ReportPhotos { get; set; } = new List<ReportPhoto>();

    public virtual User? User { get; set; }
}
