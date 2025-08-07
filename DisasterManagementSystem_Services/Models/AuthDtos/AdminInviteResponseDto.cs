namespace DisasterManagementSystem_Services.Models.AuthDtos;

public class AdminInviteResponseDto
{
    public string Email { get; set; } = default!;
    public DateTime InviteSentAt { get; set; }
    public string InviteUrl { get; set; } = default!;
}

