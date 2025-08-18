public class ImpactDto
{
    public int Id { get; set; }                // Unique identifier of the Impact
    public int DisasterEventId { get; set; }   // Related Disaster Event
    public string Type { get; set; } = null!;
    public string? Value { get; set; }
    public string? ObjectName { get; set; }
}
