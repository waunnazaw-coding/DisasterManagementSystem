using Microsoft.AspNetCore.Http;

public class ExistingPhotoUpdateDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string FilePath { get; set; }

    public IFormFile? File { get; set; }
}