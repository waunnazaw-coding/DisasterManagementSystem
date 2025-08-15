using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return null;

        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userIdString, out Guid userId))
            return userId;

        return null; // or throw exception if preferred
    }
}
