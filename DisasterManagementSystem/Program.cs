using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
using DisasterManagementSystem_Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using DisasterManagementSystem_Data.Repositories.Implements;
using DisasterManagementSystem_Data.Repositories.Interfaces;
using DisasterManagementSystem_Services;
using DisasterManagementSystem_Services.Hubs;
using DisasterManagementSystem_Services.Models;
using DisasterManagementSystem_Services.Services;
using DisasterManagementSystem_Services.Services.Implements;
using DisasterManagementSystem_Services.Services.Interfaces;
using FluentEmail.MailKitSmtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using DisasterManagementSystem_Services.Services.Implements;
using DisasterManagementSystem;
using FluentEmail.MailKitSmtp;
using DisasterManagementSystem_Services.Services.Interfaces;
using DisasterManagementSystem_Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.UseNetTopologySuite()));

// Add email settings configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Get the EmailSettings early to configure FluentEmail
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
if (emailSettings == null)
{
    throw new InvalidOperationException("EmailSettings not configured in appsettings.json");
}

builder.Services
    .AddFluentEmail(emailSettings.SenderEmail, emailSettings.SenderName) // Default sender
    .AddMailKitSender(new SmtpClientOptions
    {
        Server = emailSettings.SmtpServer,
        Port = emailSettings.SmtpPort,
        UseSsl = false,
        RequiresAuthentication = true,
        User = emailSettings.SmtpUser,
        Password = emailSettings.SmtpPass
    });

// please kindly ensure what license is appropriate for your project
QuestPDF.Settings.License = LicenseType.Community;

// Register your custom IEmailSender implementation that uses FluentEmail
builder.Services.AddTransient<IEmailSenderService , EmailSenderService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // if you use cookies or credentials
    });
});

// Add SignalR
builder.Services.AddSignalR();
builder.Services.AddHttpClient();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

// Add Authorization
builder.Services.AddAuthorization();
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReportPhotoRepository, ReportPhotoRepository>();
builder.Services.AddScoped<IDonationRepository, DonationRepository>();
builder.Services.AddScoped<IlocationRepository, LocationRepository>();
builder.Services.AddScoped<IDisasterReportRepository, DisasterReportRepository>();
builder.Services.AddScoped<IDisasterEventRepository, DisasterEventRepository>();
builder.Services.AddScoped<IDisasterTypeRepository, DisasterTypeRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAssistanceRequestRepository, AssistanceRequestRepository>();
builder.Services.AddScoped<IlocationRepository, LocationRepository>();
builder.Services.AddScoped<IDisasterEventRepository, DisasterEventRepository>();
builder.Services.AddScoped<IReliefTeamRepository, ReliefTeamRepository>();
builder.Services.AddScoped<IRequestAssignmentRepository, RequestAssignmentRepository>();    

builder.Services.AddScoped<IUserReliefTeamRepository, UserReliefTeamRepository>();
builder.Services.AddScoped<IReliefTeamsRepository, ReliefTeamsRepository>();

//builder.Services.AddScoped<IDisasterKnowledgeRepository, DisasterKnowledgeRepository>();
builder.Services.AddScoped<IFinancialAllocationRepository, FinancialAllocationRepository>();

builder.Services.AddScoped<IGdacsDisasterRepository, GdacsDisasterRepository>();
builder.Services.AddHostedService<DisasterPollingService>();
//builder.Services.AddScoped<IDisasterKnowledgeRepository, DisasterKnowledgeRepository>();
builder.Services.AddScoped<IFinancialAllocationRepository, FinancialAllocationRepository>();

builder.Services.AddScoped<IImpactRepository, ImpactRepository>();

builder.Services.AddScoped<IRequestAssignmentRepository, RequestAssignmentRepository>();
builder.Services.AddScoped<IReliefTeamActivityRepository, ReliefTeamActivityRepository>();
builder.Services.AddScoped<IPartnerRepository, PartnerRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.AddDomain();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Disaster Management API",
        Version = "v1",
        Description = "API for disaster management system",
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "support@disastermgmt.org",
            Url = new Uri("https://disastermgmt.org/support")
        }
    });

    // Add JWT Authentication support to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Optional: Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Disaster Management API v1");

        // Add the "Authorize" button to Swagger UI
        c.OAuthClientId("swagger-ui");
        c.OAuthAppName("Swagger UI");
        c.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<DisasterNotificationHub>("/disasterNotifications");

app.Run();
