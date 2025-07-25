namespace DisasterManagementSystem;

public class EmailSettings
{
    public string SmtpServer { get; set; } = null!;
    public int SmtpPort { get; set; }
    public bool UseSSL { get; set; } = true; // UseSsl property in FluentEmail.MailKit is bool
    public string SenderName { get; set; } = null!;
    public string SenderEmail { get; set; } = null!;
    public string SmtpUser { get; set; } = null!;
    public string SmtpPass { get; set; } = null!;
}