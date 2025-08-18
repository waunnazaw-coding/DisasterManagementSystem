using System;
using System.Collections.Generic;

namespace DisasterManagementSystem_Data.Models;

public partial class ReportPhoto
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public int? DisasterEventId { get; set; }

    public int? DisasterReportId { get; set; }

    public string FilePath { get; set; } = null!;

    public string FileType { get; set; } = null!;

    public long? FileSize { get; set; }

    public DateTime? UploadedAt { get; set; }

    public virtual DisasterEvent? DisasterEvent { get; set; }

    public virtual DisasterReport? DisasterReport { get; set; }
}
