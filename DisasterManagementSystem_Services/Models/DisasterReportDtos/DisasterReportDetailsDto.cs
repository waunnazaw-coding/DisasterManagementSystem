public class DisasterReportDetailsDto
{
    public int Id { get; set; }
    public int? DisasterEventId { get; set; }
    public string LocationName { get; set; }
    public string? AddressDetail { get; set; }
    public string Type { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; } = "Pending";
    public string? Severity { get; set; }
    public string? Source { get; set; }
    public string LocationGeoJson { get; set; }
    public List<ReportPhotoDto> ReportPhotos { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}