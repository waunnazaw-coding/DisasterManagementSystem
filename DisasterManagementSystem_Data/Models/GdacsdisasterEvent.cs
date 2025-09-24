using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class GdacsdisasterEvent
{
    public string EventId { get; set; } = null!;

    public string? EventType { get; set; }

    public string? Severity { get; set; }

    public DateTime? EventDate { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? LocationAddress { get; set; }

    public string? Impact { get; set; }

    public string? Status { get; set; }
}
