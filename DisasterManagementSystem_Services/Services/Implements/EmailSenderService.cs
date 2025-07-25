using DisasterManagementSystem_Services.Services.Interfaces;
using DisasterManagementSystem;
using FluentEmail.Core;
using Microsoft.Extensions.Options;

namespace DisasterManagementSystem_Services.Services.Implements;

public class EmailSenderService : IEmailSenderService
{
    private readonly IFluentEmailFactory _emailFactory;
    private readonly EmailSettings _emailSettings;
    
    public EmailSenderService(IFluentEmailFactory emailFactory, IOptions<EmailSettings> options)
    {
        _emailFactory = emailFactory;
        _emailSettings = options.Value;
    }
    
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var emailMessage = _emailFactory.Create()
            .To(email)
            .Subject(subject)
            .Body(htmlMessage, true)
            .SetFrom(_emailSettings.SenderEmail, _emailSettings.SenderName);

        var response = await emailMessage.SendAsync();

        if (!response.Successful)
        {
            throw new Exception($"Failed to send email: {string.Join(", ", response.ErrorMessages)}");
        }
    }
}