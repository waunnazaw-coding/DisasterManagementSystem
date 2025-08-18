public class ImpactSummaryDto
{
    public string Type { get; set; }
    public string ObjectName { get; set; }
    public decimal TotalValue { get; set; }
    public List<string> Descriptions { get; set; } = new();
}
