using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class Impact
{
    public int Id { get; set; }

    public int? DisasterEventId { get; set; }

    public int? DisasterReportId { get; set; }

    public string Type { get; set; } = null!;

    public string? Value { get; set; }

    public string? ObjectName { get; set; }

    public virtual DisasterEvent? DisasterEvent { get; set; }

    public virtual DisasterReport? DisasterReport { get; set; }
}
