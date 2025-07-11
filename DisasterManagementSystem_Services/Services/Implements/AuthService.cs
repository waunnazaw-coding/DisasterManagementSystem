
using System.Security.Claims;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Data.Repositories.Interfaces;
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
    private readonly IConfiguration _configuration; // Inject IConfiguration

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IConfiguration configuration)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration)); // Inject and assign
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var existingUser = await _userRepository.GetByEmailAsync(model.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            AuthProvider = "Manual", // Set auth provider for manual registration
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        var tokens = await _jwtService.GenerateTokensAsync(user);

        return MapToAuthResponseDto(tokens);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var user = await _userRepository.GetByEmailAsync(model.Email);
        // Ensure user is not a social login user trying to login manually without password
        if (user == null || user.AuthProvider != "Manual" || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var tokens = await _jwtService.GenerateTokensAsync(user);
        return MapToAuthResponseDto(tokens);
    }

    public async Task<UserResponseDto> GetMeAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public async Task<UserResponseDto> GetMeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            // Profile = user.Profile // Uncomment if user has Profile property
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentNullException(nameof(accessToken));
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken));

        var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
            throw new SecurityTokenException("Invalid access token.");
    
        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out Guid userId))
            throw new SecurityTokenException("Invalid token claims: User ID not found or invalid format.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.RefreshToken == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            throw new SecurityTokenException("Invalid or expired refresh token.");

        var tokens = await _jwtService.GenerateTokensAsync(user);
        return MapToAuthResponseDto(tokens);
    }


    // NEW: Google Login Implementation
    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (string.IsNullOrWhiteSpace(model.IdToken))
            throw new ArgumentNullException(nameof(model.IdToken));

        GoogleJsonWebSignature.Payload payload;
        try
        {
            // Validate the Google ID token. Audience must match your Google Client ID.
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { _configuration["Authentication:Google:ClientId"] }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, validationSettings);
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedAccessException("Invalid Google ID token.", ex);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException("Google authentication failed.", ex);
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
        return MapToAuthResponseDto(tokens);
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
