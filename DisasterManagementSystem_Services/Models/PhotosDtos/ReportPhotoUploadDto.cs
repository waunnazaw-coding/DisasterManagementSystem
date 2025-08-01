using Microsoft.AspNetCore.Http;

public class ReportPhotoUploadDto
{
    public int DisasterEventId { get; set; }
    public IFormFile File { get; set; }
    public string Description { get; set; }
    public string CreatedUserId { get; set; }
}
