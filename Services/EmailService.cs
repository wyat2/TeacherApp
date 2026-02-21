using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TeacherApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"] ?? throw new InvalidOperationException("SmtpServer is not configured");
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"] ?? throw new InvalidOperationException("SenderEmail is not configured");
                var senderName = emailSettings["SenderName"] ?? "TeacherApp";
                var username = emailSettings["Username"] ?? throw new InvalidOperationException("Username is not configured");
                var password = emailSettings["Password"] ?? throw new InvalidOperationException("Password is not configured");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                throw;
            }
        }
    }
}
