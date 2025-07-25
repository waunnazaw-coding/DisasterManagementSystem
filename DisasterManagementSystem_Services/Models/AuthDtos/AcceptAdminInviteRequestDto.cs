namespace DisasterManagementSystem_Services.Models.AuthDtos;

public class AcceptAdminInviteRequestDto
{
    public string Email { get; set; } = default!;
    public string Token { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}