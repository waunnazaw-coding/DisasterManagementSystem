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
