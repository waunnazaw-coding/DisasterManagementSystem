public class DisasterEventDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string DisasterTypeName { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public string LocationName { get; set; } = "";
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Severity { get; set; }
    public string Status { get; set; } = "";
    public string? Source { get; set; }
    public string Description { get; set; }
    public string LocationGeoJson { get; set; }

    public string? CreatedUserName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedUserName { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<ReportPhotoDto> ReportPhotos { get; set; }


    public List<ImpactSummaryDto> ImpactSummaries { get; set; } // new property
}