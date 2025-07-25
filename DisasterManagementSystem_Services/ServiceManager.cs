using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Services.Services.Implements;
using DisasterManagementSystem_Services.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using DisasterManagementSystem_Services.Services.Interfaces;

namespace DisasterManagementSystem_Services
{
    public static class ServicesManager
    {
        public static void AddDomain(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IlocationService, LocationService>();
            builder.Services.AddScoped<IDisasterReportService, DisasterReportService>();
            builder.Services.AddScoped<IDisasterTypeService, DisasterTypeService>();

            builder.Services.AddHttpClient<INominatimGeocodingService, NominatimGeocodingService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IReportPhotoService, ReportPhotoService>();
        }
    }
}
