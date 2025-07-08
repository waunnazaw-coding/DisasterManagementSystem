using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class AssistanceRequest
{
    public int Id { get; set; }

    public int? DisasterEventId { get; set; }

    public int? DisasterReportId { get; set; }

    public Guid? UserId { get; set; }

    public int? LocationId { get; set; }

    public string SupportType { get; set; } = null!;

    public int? Quantity { get; set; }

    public string? Unit { get; set; }

    public string? Description { get; set; }

    public string? Priority { get; set; }

    public string Status { get; set; } = null!;

    public string? ContactName { get; set; }

    public string? Email { get; set; }

    public string? ContactPhone { get; set; }

    public string? DetailedAddress { get; set; }

    public string? Source { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? FulfilledAt { get; set; }

    public virtual DisasterEvent? DisasterEvent { get; set; }

    public virtual DisasterReport? DisasterReport { get; set; }

    public virtual ICollection<DonationDistribution> DonationDistributions { get; set; } = new List<DonationDistribution>();

    public virtual Location? Location { get; set; }

    public virtual ICollection<RequestAssignment> RequestAssignments { get; set; } = new List<RequestAssignment>();

    public virtual User? User { get; set; }
}
