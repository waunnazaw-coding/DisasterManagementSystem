using System;
using System.Threading.Tasks;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.AuthDtos;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto model);
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto model);
        Task<Result<UserResponseDto>> GetMeAsync(Guid userId);
        Task<Result<AuthResponseDto>> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<Result<AuthResponseDto>> GoogleLoginAsync(GoogleLoginDto model);
    }
}
