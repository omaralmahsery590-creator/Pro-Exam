using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace pro_exam.email_service
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "465");
            _smtpUser = _configuration["EmailSettings:SmtpUser"] ?? throw new InvalidOperationException("SMTP user not configured");
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? throw new InvalidOperationException("SMTP password not configured");
            _fromEmail = _configuration["EmailSettings:FromEmail"] ?? _smtpUser;
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            toEmail = toEmail?.Trim() ?? throw new ArgumentNullException(nameof(toEmail));

            if (string.IsNullOrWhiteSpace(toEmail) || !toEmail.Contains('@'))
                throw new ArgumentException($"Invalid email address: '{toEmail}'", nameof(toEmail));

            _logger.LogInformation("Attempting to send email to {Email} with subject: {Subject}", toEmail, subject);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Pro Exam", _fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(_smtpUser, _smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}. Error: {Message}", toEmail, ex.Message);
                throw;
            }
        }

        public Task SendVerificationEmailAsync(string email, string verificationCode)
            => SendEmailAsync(email, "Email Verification", $"Your verification code is: {verificationCode}");

        public Task SendPasswordAsync(string email, string password)
            => SendEmailAsync(email, "Your Password", $"Your Password is: {password}");

        public Task SendForgetPasswordAsync(string email, string verificationCode)
            => SendEmailAsync(email, "Forgot Password", $"Your verification code is: {verificationCode}");

        public Task SendStatus(string email, string body)
            => SendEmailAsync(email, "Respond to the joining request", body);
    }
}