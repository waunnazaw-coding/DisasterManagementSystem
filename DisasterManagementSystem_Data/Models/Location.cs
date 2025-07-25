using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace DisasterManagementSystem_Data.Models;

public partial class Location
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public Geometry? Geography { get; set; }

    public string? Country { get; set; }

    public string? Region { get; set; }

    public virtual ICollection<AssistanceRequest> AssistanceRequests { get; set; } = new List<AssistanceRequest>();

    public virtual ICollection<DisasterEvent> DisasterEvents { get; set; } = new List<DisasterEvent>();

    public virtual ICollection<DisasterReport> DisasterReports { get; set; } = new List<DisasterReport>();

    public virtual ICollection<ReliefTeam> ReliefTeams { get; set; } = new List<ReliefTeam>();
}
