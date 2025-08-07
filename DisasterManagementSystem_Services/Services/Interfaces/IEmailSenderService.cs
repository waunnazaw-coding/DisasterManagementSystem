namespace DisasterManagementSystem_Services.Services.Interfaces;

public interface IEmailSenderService
{
    Task SendEmailAsync(string email, string subject, string message);
}