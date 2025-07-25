namespace DisasterManagementSystem_Services.Models.AuthDtos;

public class AcceptAdminInviteResponseDto
{
    public string Email { get; set; } = default!;
    public bool IsAdmin { get; set; }
}