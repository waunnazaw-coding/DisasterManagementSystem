using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Services.Services.Implements;
using DisasterManagementSystem_Services.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using DisasterManagementSystem_Services.Service;
using DisasterManagementSystem_Services.Services.Implements.DisasterManagementSystem_Services.Services.Implements;

namespace DisasterManagementSystem_Services.Services
{
    public static class ServicesManager
    {
        public static void AddDomain(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IlocationService, LocationService>();
            builder.Services.AddScoped<IDisasterReportService, DisasterReportService>();
            builder.Services.AddScoped<IDisasterEventService, DisasterEventService>();
            builder.Services.AddScoped<IDisasterTypeService, DisasterTypeService>();
            builder.Services.AddScoped<IImpactService, ImpactService>();

            builder.Services.AddHttpClient<INominatimGeocodingService, NominatimGeocodingService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IReportPhotoService, ReportPhotoService>();
            builder.Services.AddScoped<IDonationService,DonationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IAssistanceRequestService, AssistanceRequestService>();
            builder.Services.AddScoped<IDisasterEventService, DisasterEventService>();
            builder.Services.AddScoped<IReliefTeamService, ReliefTeamService>();
            builder.Services.AddScoped<IRequestAssignmentService, RequestAssignmentService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
            builder.Services.AddScoped<IReliefTeamsService, ReliefTeamsService>();
            builder.Services.AddScoped<IReliefTeamActivityService, ReliefTeamActivityService>();

            builder.Services.AddScoped<IFinancialAllocationService, FinancialAllocationService>();
            builder.Services.AddScoped<IReliefTeamService, ReliefTeamService>();
            //builder.Services.AddScoped<IDisasterKnowledgeService, DisasterKnowledgeService>();

            builder.Services.AddHttpContextAccessor(); // Important for IHttpContextAccessor
            builder.Services.AddScoped<IUserContextService, UserContextService>();
        }
    }
}
