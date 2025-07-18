public class DisasterReportUpdateDto
{
    public int Id { get; set; }
    public string? AddressDetail { get; set; }
    public string Type { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
}
