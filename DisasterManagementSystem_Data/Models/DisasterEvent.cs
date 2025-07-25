using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class DisasterEvent
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int DisasterTypeId { get; set; }

    public DateOnly StartDate { get; set; }

    public int LocationId { get; set; }

    public string? Severity { get; set; }

    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedUserId { get; set; }

    public Guid? UpdatedUserId { get; set; }

    public virtual ICollection<AssistanceRequest> AssistanceRequests { get; set; } = new List<AssistanceRequest>();

    public virtual ICollection<DisasterReport> DisasterReports { get; set; } = new List<DisasterReport>();

    public virtual DisasterType DisasterType { get; set; } = null!;

    public virtual ICollection<Impact> Impacts { get; set; } = new List<Impact>();

    public virtual Location Location { get; set; } = null!;

    public virtual ICollection<ReportPhoto> ReportPhotos { get; set; } = new List<ReportPhoto>();
}
