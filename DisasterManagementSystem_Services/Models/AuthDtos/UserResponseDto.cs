namespace DisasterManagementSystem_Services.Models.AuthDtos;

public class UserResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string Role { get; set; }

    public string? Profile { get; set; }

    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }

    public DateTime AccessTokenExpiration { get; set; }
}