using DisasterManagementSystem_Data.Repositories.Implements;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DisasterManagementSystem_Services
{
    public static class ServicesManager
    {
        public static void AddDomain(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ITestService, TestService>();
            builder.Services.AddScoped<IlocationService, LocationService>();
            builder.Services.AddHttpClient<INominatimGeocodingService, NominatimGeocodingService>();
        }
    }
}
