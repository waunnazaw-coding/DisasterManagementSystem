using System.Security.Claims;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Services.Models.AuthDtos;

namespace DisasterManagementSystem_Data.Repositories;

public interface IJwtService
{
    string GenerateToken(Guid userId, string role, TimeSpan expiresIn);
    string GenerateAdminInviteToken(Guid userId, TimeSpan expiresIn);

    Task<AuthResponseDto> GenerateTokensAsync(User user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    //string GenerateAccessToken(User user);
    //string GenerateRefreshToken();
    //ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}