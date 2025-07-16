namespace DisasterManagementSystem_Services.Models.AuthDtos;

public class UserResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? Profile { get; set; }
}