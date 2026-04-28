using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

            // قراءة الإعدادات من appsettings.json بدلاً من hardcoding
            _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUser = _configuration["EmailSettings:SmtpUser"] ?? throw new InvalidOperationException("SMTP user not configured");
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? throw new InvalidOperationException("SMTP password not configured");
            _fromEmail = _configuration["EmailSettings:FromEmail"] ?? _smtpUser;
        }

        // إنشاء SmtpClient جديد لكل عملية إرسال (آمن أكثر مع async)
        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(_smtpHost)
            {
                Port = _smtpPort,
                Credentials = new NetworkCredential(_smtpUser, _smtpPassword),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 20000 // 20 seconds
            };
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            toEmail = toEmail?.Trim() ?? throw new ArgumentNullException(nameof(toEmail));

            if (string.IsNullOrWhiteSpace(toEmail) || !toEmail.Contains('@'))
                throw new ArgumentException($"Invalid email address: '{toEmail}'", nameof(toEmail));

            _logger.LogInformation("Attempting to send email to {Email} with subject: {Subject}", toEmail, subject);

            using var smtpClient = CreateSmtpClient();
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            };
            mailMessage.To.Add(toEmail);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email to {Email}. StatusCode: {StatusCode}, Message: {Message}",
                    toEmail, ex.StatusCode, ex.Message);
                throw; // re-throw عشان الكونترولر يعرف إنه فشل
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {Email}", toEmail);
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