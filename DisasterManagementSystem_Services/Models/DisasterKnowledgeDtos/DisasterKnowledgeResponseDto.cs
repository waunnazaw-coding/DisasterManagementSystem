namespace DisasterManagementSystem_Services.Models.DisasterKnowledgeDtos;

public class DisasterKnowledgeResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string? DisasterType { get; set; }
    public int? AuthorId { get; set; }
    public string? Content { get; set; }
    public string Language { get; set; } = null!;
    public string? ReferenceFrom { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public List<ResourceResponseDto>? Resources { get; set; }
}

public class ResourceResponseDto
{
    public int Id { get; set; }
    public string ResourceType { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime DateAdded { get; set; }
}
