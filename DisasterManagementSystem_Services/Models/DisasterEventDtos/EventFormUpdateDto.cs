using DisasterManagementSystem_Services.Models;
using Microsoft.AspNetCore.Http;

public class EventFormUpdateDto
{
    public int Id { get; set; }

    // ---------- Location Info ----------
    public string LocationName { get; set; } = string.Empty;
    public string GeoJson { get; set; } = string.Empty;

    // ---------- Event Info ----------
    public string Name { get; set; } = string.Empty;
    public int DisasterTypeId { get; set; }
    public DateOnly? StartDate { get; set; }
    public string? Severity { get; set; }
    public string? Description { get; set; }

    // ---------- Photo Handling ----------

    /// <summary>
    /// New files to be uploaded
    /// </summary>
    public IFormFile[] NewPhotos { get; set; } = Array.Empty<IFormFile>();
    public List<string> NewPhotoDescription { get; set; } = new List<string>();


    /// <summary>
    /// IDs of existing photos the user has marked for deletion
    /// </summary>
    public List<int> DeletedPhotoIds { get; set; } = new();

    /// <summary>
    /// List of existing photos to keep (not deleted)
    /// </summary>
    public List<ExistingPhotoUpdateDto> ExistingPhotos { get; set; } = new();
}
