namespace DisasterManagementSystem_Services.Models.AuthDtos;

public class AuthResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }

    public DateTime AccessTokenExpiration { get; set; }
}