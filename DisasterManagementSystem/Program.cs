using DisasterManagementSystem;
using DisasterManagementSystem_Data.Models;
using DisasterManagementSystem_Data.Repositories;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System;
using System.Security.Claims;
using System.Text;
using System.Text;
using System.Text.Json.Serialization;


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

//Add Authorization (optional, but recommended)
builder.Services.AddAuthorization();
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReportPhotoRepository, ReportPhotoRepository>();
builder.Services.AddScoped<IDonationRepository, DonationRepository>();
// Repositories
builder.Services.AddScoped<IlocationRepository, LocationRepository>();
builder.Services.AddScoped<IDisasterReportRepository, DisasterReportRepository>();
builder.Services.AddScoped<IDisasterTypeRepository, DisasterTypeRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAssistanceRequestRepository, AssistanceRequestRepository>();
builder.Services.AddScoped<IlocationRepository,LocationRepository>();
builder.Services.AddScoped<IDisasterEventRepository, DisasterEventRepository>();
builder.Services.AddScoped<IDisasterEventRepository, DisasterEventRepository>();
builder.Services.AddScoped<IReliefTeamRepository, ReliefTeamRepository>();
builder.Services.AddScoped<IRequestAssignmentRepository, RequestAssignmentRepository>();    

builder.Services.AddScoped<IUserReliefTeamRepository, UserReliefTeamRepository>();
builder.Services.AddScoped<IReliefTeamsRepository, ReliefTeamsRepository>();

//builder.Services.AddScoped<IDisasterKnowledgeRepository, DisasterKnowledgeRepository>();
builder.Services.AddScoped<IFinancialAllocationRepository, FinancialAllocationRepository>();


builder.AddDomain();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

// IMPORTANT: Add Authentication middleware BEFORE Authorization middleware
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
