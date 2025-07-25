using DisasterManagementSystem_Services.Services.Interfaces;

namespace DisasterManagementSystem_Services.Services.Implements;

public class EmailSenderSenderService : IEmailSenderService
{
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        throw new NotImplementedException();
    }
}