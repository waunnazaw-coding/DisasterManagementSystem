public class DeletedReportLog
{
    public int Id { get; set; }
    public int ReportId { get; set; }   // Original DisasterReport Id
    public string ReportName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
    public string DeletedBy { get; set; } = "System"; // since cleanup service deletes it
    public string? ExtraInfo { get; set; } // JSON string (Impacts count, Photos count, etc.)
}
