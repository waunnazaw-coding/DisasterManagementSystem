public class ImpactCreateDto
{
    public int DisasterEventId { get; set; }
    public string Type { get; set; } = null!;
    public string? Value { get; set; }
    public string? ObjectName { get; set; }
}
