namespace DisasterManagementSystem_Services.Models.DisasterTypsDtos;
public class DisasterTypeUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
}
