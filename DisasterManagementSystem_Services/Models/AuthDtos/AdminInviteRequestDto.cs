namespace DisasterManagementSystem_Services.Models.AuthDtos;

public class AdminInviteRequestDto
{
    public string Email { get; set; } = default!;
    public string? Name { get; set; }
}