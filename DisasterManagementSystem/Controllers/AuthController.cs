using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Models.AuthDtos;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisasterManagementSystem_Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        
        [HttpPost("register")]
        public async Task<IResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);
            return result.Execute();
        }

        
        [HttpPost("login")]
        public async Task<IResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var result = await _authService.LoginAsync(model);
            return result.Execute();
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IResult> Logout()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Results.Unauthorized();
            }
            var result = await _authService.LogoutAsync(userId);
            if (!result.IsSuccess)
                return Results.BadRequest(new { message = result.Message });
            return Results.Ok(new { message = "Logout successful" });
        }



        [HttpPost("refresh-token")]
        public async Task<IResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
            return result.Execute();
        }


        // Admin invite endpoint - only Admins should call this
        [HttpPost("admin-invite")]
        //[Authorize(Roles = "Admin,SysAdmin")]
        public async Task<IResult> SendAdminInvite([FromBody] AdminInviteRequestDto inviteDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);


            var result = await _authService.SendAdminInviteAsync(inviteDto);

            if (!result.IsSuccess)
                return Results.BadRequest(new { message = result.Message });

            return result.Execute();
        }

        [HttpPost("disaster-management-admin")]
        //[Authorize(Roles = "Admin,SysAdmin")]
        public async Task<IResult> SendDisasterManagementAdminInvite([FromBody] AdminInviteRequestDto inviteDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);


            var result = await _authService.SendAdminInviteAsync(inviteDto);

            if (!result.IsSuccess)
                return Results.BadRequest(new { message = result.Message });

            return result.Execute();
        }


        [HttpPost("financial-admin")]
        //[Authorize(Roles = "Admin,SysAdmin")]
        public async Task<IResult> SendFinancialAdminInvite([FromBody] AdminInviteRequestDto inviteDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);


            var result = await _authService.SendAdminInviteAsync(inviteDto);

            if (!result.IsSuccess)
                return Results.BadRequest(new { message = result.Message });

            return result.Execute();
        }

        // Accept invite & reset password endpoint - PATCH since partial update
        [HttpPatch("accept-admin-invite")]
        [AllowAnonymous]
        public async Task<IResult> AcceptAdminInvite([FromBody] AcceptAdminInviteRequestDto acceptDto)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var result = await _authService.AcceptAdminInviteAsync(acceptDto);

            if (!result.IsSuccess)
                return Results.BadRequest(new { message = result.Message });

            return result.Execute();
        }
        
        
        [HttpPatch("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.ResetPasswordAsync(dto);

            if (!response.Success)
                return BadRequest(new { message = response.Message });

            // Return 200 OK with success message
            return Ok(new { message = response.Message });
        }

        
        [HttpPost("google-login")]
        public async Task<IResult> GoogleLogin([FromBody] GoogleLoginDto model)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            var result = await _authService.GoogleLoginAsync(model);
            return result.Execute();
        }
        
        
        [Authorize]
        [HttpGet("profile")]
        public async Task<IResult> GetMe()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Results.Unauthorized();
            }

            var result = await _authService.GetMeAsync(userId);
            return result.Execute();
        }

    }
}
