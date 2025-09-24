using Microsoft.AspNetCore.Http;

namespace DisasterManagementSystem_Services.Models.DisasterKnowledgeDtos;

public class DisasterKnowledgeRequestDto
{
    public string Title { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string? DisasterType { get; set; }
    public int? AuthorId { get; set; }
    public string? Content { get; set; }
    public string Language { get; set; } = null!;
    public string? ReferenceFrom { get; set; }
}


public class ResourceRequestDto
{
    public string ResourceType { get; set; } = null!;
    public IFormFile? File { get; set; }
    public string? Description { get; set; }
}