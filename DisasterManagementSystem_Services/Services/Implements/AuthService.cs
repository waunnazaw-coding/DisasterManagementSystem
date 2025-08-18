using System.Security.Claims;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.AuthDtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DisasterManagementSystem_Services.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IEmailSenderService _emailSender;

        public AuthService(IUserRepository userRepository, IJwtService jwtService, IConfiguration configuration , IEmailSenderService emailSender)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _emailSender = emailSender;
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

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Result<AuthResponseDto>.NotFoundError("Invalid credentials.");

            var tokens = await _jwtService.GenerateTokensAsync(user);
            return Result<AuthResponseDto>.Success(MapToAuthResponseDto(tokens));
        }


        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            try
            {
                var principal = _jwtService.GetPrincipalFromExpiredToken(dto.Token);
                if (principal == null)
                    return new ResetPasswordResponseDto { Success = false, Message = "Invalid or expired token." };

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return new ResetPasswordResponseDto { Success = false, Message = "Invalid token claims." };

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
                    return new ResetPasswordResponseDto { Success = false, Message = "User not found or email mismatch." };

                //var updatedUser = new User
                //{
                //    Id = user.Id,
                //    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword),
                //    AuthProvider = null
                //};

                //// Attach the user and mark only specific fields modified
                //_userRepository.Attach(updatedUser);
                //var entry = _userRepository.Entry(updatedUser);
                //entry.Property(u => u.PasswordHash).IsModified = true;
                //entry.Property(u => u.AuthProvider).IsModified = true;

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.AuthProvider = null;
                await _userRepository.SaveChangesAsync();

                return new ResetPasswordResponseDto { Success = true, Message = "Password reset successful." };
            }
            catch (Exception ex)
            {

                return new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = "An unexpected error occurred while resetting the password."
                };
            }
        }



       public async Task<Result<AdminInviteResponseDto>> SendAdminInviteAsync(AdminInviteRequestDto inviteDto)
{
    try
    {
        var user = await _userRepository.GetByEmailAsync(inviteDto.Email);

        if (user == null)
        {
            user = new User
            {
                Email = inviteDto.Email,
                Name = inviteDto.Name ?? "",
                Role = "User",        // default role
                Status = "Active",    // custom status tracking
                CreatedAt = DateTime.UtcNow,
                AuthProvider = null
            };

            await _userRepository.AddAsync(user);
        }
        else
        {
            if (user.Role == "Admin")
                return Result<AdminInviteResponseDto>.Failure("User is already an admin.");

            user.Status = "Active";
            await _userRepository.UpdateAsync(user);
        }

        var inviteToken = _jwtService.GenerateAdminInviteToken(user.Id, TimeSpan.FromHours(24));

        var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";

        // Construct invite URL dynamically with the token as query parameter
        var inviteUrl = $"{frontendBaseUrl.TrimEnd('/')}/accept-invite?token={Uri.EscapeDataString(inviteToken)}&email={Uri.EscapeDataString(user.Email)}";

                var subject = "You are invited to become an Admin";
        var htmlMessage = $@"
            <p>Hello {(!string.IsNullOrEmpty(user.Name) ? user.Name : user.Email)},</p>
            <p>You have been invited to join as an <strong>Admin</strong> on our system.</p>
            <p>Please click the link below to set your password and activate your admin account:</p>
            <p><a href='{inviteUrl}'>Accept Admin Invitation</a></p>
            <p>This link will expire in 24 hours.</p>
            <p>If you did not expect this invitation, please ignore this email.</p>
        ";

        await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);

        var responseDto = new AdminInviteResponseDto
        {
            Email = user.Email,
            InviteSentAt = DateTime.UtcNow,
            InviteUrl = inviteUrl
        };

        return Result<AdminInviteResponseDto>.Success(responseDto);
    }
    catch (Exception ex)
    {
        // TODO: Replace with your preferred logging
        // _logger.LogError(ex, "Failed to send admin invite to {Email}", inviteDto.Email);
        Console.Error.WriteLine($"Error in SendAdminInviteAsync: {ex}");

        return Result<AdminInviteResponseDto>.Failure("Failed to send admin invite. Please try again later.");
    }
}



        public async Task<Result<AcceptAdminInviteResponseDto>> AcceptAdminInviteAsync(AcceptAdminInviteRequestDto acceptDto)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(acceptDto.Token);
            if (principal == null)
                return Result<AcceptAdminInviteResponseDto>.Failure("Invalid or expired invite token.");

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Result<AcceptAdminInviteResponseDto>.Failure("Invalid token claims.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !string.Equals(user.Email, acceptDto.Email, StringComparison.OrdinalIgnoreCase))
                return Result<AcceptAdminInviteResponseDto>.Failure("User not found or email mismatch.");

            //var updatedUser = new User
            //{
            //    Id = user.Id,
            //    PasswordHash = BCrypt.Net.BCrypt.HashPassword(acceptDto.NewPassword),
            //    AuthProvider = null,
            //    Role = "Admin",
            //    Status = "Active"
            //};

            //_userRepository.Attach(updatedUser);
            //var entry = _userRepository.Entry(updatedUser);
            //entry.Property(u => u.PasswordHash).IsModified = true;
            //entry.Property(u => u.AuthProvider).IsModified = true;
            //entry.Property(u => u.Role).IsModified = true;
            //entry.Property(u => u.Status).IsModified = true;


            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(acceptDto.NewPassword);
            user.AuthProvider = null;
            user.Role = "Admin";
            user.Status = "Active";
            await _userRepository.SaveChangesAsync();

            var responseDto = new AcceptAdminInviteResponseDto
            {
                Email = user.Email,
                IsAdmin = true,
            };

            return Result<AcceptAdminInviteResponseDto>.Success(responseDto);
        }

        public async Task<Result<OperationResponseDto>> LogoutAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return  Result < OperationResponseDto >.Failure("User not found.");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return Result<OperationResponseDto>.Success(new OperationResponseDto { Message = "Logout Successfully." });
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
                 //Profile = user.Profile // Uncomment if applicable
                Role = user.Role,
                //RefreshToken = user.RefreshToken
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
