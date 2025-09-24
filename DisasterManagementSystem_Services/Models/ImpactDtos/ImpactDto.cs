public class ImpactDto
{
    public int Id { get; set; }                  // Unique identifier of the Impact
    public string RelatedEvent { get; set; }    // Related Disaster Event
    public string RelatedReport { get; set; }
    public int? DisasterReportId { get; set; }   // Related Disaster Report
    public string Type { get; set; } = null!;
    public string? Value { get; set; }
    public string? ObjectName { get; set; }
    public string? Status { get; set; }
}