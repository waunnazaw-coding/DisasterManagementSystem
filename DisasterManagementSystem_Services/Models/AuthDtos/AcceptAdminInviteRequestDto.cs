namespace DisasterManagementSystem_Services.Models.AuthDtos;

public class AcceptAdminInviteRequestDto
{
    public string Email { get; set; } 
    public string Token { get; set; } 
    public string NewPassword { get; set; }
}