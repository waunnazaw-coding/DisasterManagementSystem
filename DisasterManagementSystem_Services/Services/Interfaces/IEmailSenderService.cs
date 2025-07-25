namespace DisasterManagementSystem_Services.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
}