using System.Security.Claims;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.AuthDtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IJwtService jwtService, IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto model)
        {
            if (model == null)
                return Result<AuthResponseDto>.Failure("Registration data cannot be null.");

            var existingUser = await _userRepository.GetByEmailAsync(model.Email);
            if (existingUser != null)
                return Result<AuthResponseDto>.Failure("Email already registered.");

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                AuthProvider = "Manual",
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            var tokens = await _jwtService.GenerateTokensAsync(user);

            return Result<AuthResponseDto>.Success(MapToAuthResponseDto(tokens));
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto model)
        {
            if (model == null)
                return Result<AuthResponseDto>.Failure("Login data cannot be null.");

            var user = await _userRepository.GetByEmailAsync(model.Email);

            if (user == null || user.AuthProvider != "Manual" || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Result<AuthResponseDto>.Failure("Invalid credentials.");

            var tokens = await _jwtService.GenerateTokensAsync(user);
            return Result<AuthResponseDto>.Success(MapToAuthResponseDto(tokens));
        }


        public async Task<Result<UserResponseDto>> GetMeAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<UserResponseDto>.NotFoundError("User not found.");

            var userDto = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt=user.CreatedAt,
                
                // Profile = user.Profile // Uncomment if applicable
            };

            return Result<UserResponseDto>.Success(userDto);
        }

        public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return Result<AuthResponseDto>.Failure("Access token is required.");

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Result<AuthResponseDto>.Failure("Refresh token is required.");

            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                return Result<AuthResponseDto>.Failure("Invalid access token.");

            var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Result<AuthResponseDto>.Failure("Invalid token claims: User ID not found or invalid format.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<AuthResponseDto>.NotFoundError("User not found.");

            if (user.RefreshToken == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.");

            var tokens = await _jwtService.GenerateTokensAsync(user);
            return Result<AuthResponseDto>.Success(MapToAuthResponseDto(tokens));
        }

        public async Task<Result<AuthResponseDto>> GoogleLoginAsync(GoogleLoginDto model)
        {
            if (model == null)
                return Result<AuthResponseDto>.Failure("Google login data cannot be null.");

            if (string.IsNullOrWhiteSpace(model.IdToken))
                return Result<AuthResponseDto>.Failure("Google ID token is required.");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _configuration["Authentication:Google:ClientId"] }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, validationSettings);
            }
            catch (InvalidJwtException ex)
            {
                return Result<AuthResponseDto>.Failure("Invalid Google ID token.");
            }
            catch (Exception ex)
            {
                return Result<AuthResponseDto>.Failure("Google authentication failed.");
            }

            var user = await _userRepository.GetByExternalIdAsync(payload.Subject, "Google");

            if (user == null)
            {
                user = new User
                {
                    Name = payload.Name ?? payload.Email,
                    Email = payload.Email,
                    AuthProvider = "Google",
                    ExternalId = payload.Subject,
                    CreatedAt = DateTime.UtcNow
                };
                await _userRepository.AddAsync(user);
            }
            else
            {
                user.Name = payload.Name ?? user.Name;
                user.Email = payload.Email ?? user.Email;
                await _userRepository.UpdateAsync(user);
            }

            var tokens = await _jwtService.GenerateTokensAsync(user);
            return Result<AuthResponseDto>.Success(MapToAuthResponseDto(tokens));
        }

        private AuthResponseDto MapToAuthResponseDto(AuthResponseDto tokens)
        {
            return new AuthResponseDto
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                AccessTokenExpiration = tokens.AccessTokenExpiration
            };
        }
    }
}
