namespace DisasterManagementSystem_Services.Services.Interfaces;

public interface IEmailSenderService
{
    Task SendEmailAsync(string email, string subject, string message);

    Task SendEmailAsync(IEnumerable<string> emails, string subject, string htmlMessage);
}