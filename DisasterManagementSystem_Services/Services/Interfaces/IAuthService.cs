using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisasterManagementSystem_Services.Models.AuthDtos;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
        Task<UserResponseDto> GetMeAsync(int userId);
        Task<AuthResponseDto> RefreshTokenAsync(string accessToken, string refreshToken);
        // NEW: Method for Google Login
        Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto model);
    }
}
