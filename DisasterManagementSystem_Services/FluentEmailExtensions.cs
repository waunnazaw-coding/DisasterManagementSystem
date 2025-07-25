using DisasterManagementSystem;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DisasterManagementSystem_Services;

public static class FluentEmailExtensions
{
    public static void AddFluentEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
        services.AddFluentEmail(emailSettings.SenderEmail, emailSettings.SenderName)
            .AddSmtpSender(emailSettings.SmtpServer, emailSettings.SmtpPort, emailSettings.SmtpUser, emailSettings.SmtpPass);
    }
}
