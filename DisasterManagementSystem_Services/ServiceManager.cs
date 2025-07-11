using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Data.Repositories.Implements;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services.Services.Implements;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DisasterManagementSystem_Services
{
    public static class ServicesManager
    {
        public static void AddDomain(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
        }
    }
}
