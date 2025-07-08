using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class RequestAssignment
{
    public int Id { get; set; }

    public int AssistanceRequestId { get; set; }

    public int ReliefTeamId { get; set; }

    public Guid? AssignedBy { get; set; }

    public DateTime? AssignedAt { get; set; }

    public string Status { get; set; } = null!;

    public string? Priority { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Notes { get; set; }

    public Guid? LastUpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? AssignedByNavigation { get; set; }

    public virtual AssistanceRequest AssistanceRequest { get; set; } = null!;

    public virtual User? LastUpdatedByNavigation { get; set; }

    public virtual ReliefTeam ReliefTeam { get; set; } = null!;
}
