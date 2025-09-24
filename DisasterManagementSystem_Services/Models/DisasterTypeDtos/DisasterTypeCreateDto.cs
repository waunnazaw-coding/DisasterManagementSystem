namespace DisasterManagementSystem_Services.Models.DisasterTypsDtos;
public class DisasterTypeCreateDto
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!; // "Natural" or "Artificial Disaster"
    public string? Description { get; set; }
}
