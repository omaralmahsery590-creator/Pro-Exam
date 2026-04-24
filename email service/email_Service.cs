using System.Net.Mail;
using System.Net;

namespace pro_exam.email_service
{
    public class EmailService
    {
        private readonly SmtpClient _smtpClient;

        public EmailService()
        {
            _smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("itcollegeschedule@gmail.com", "ivnj iotd krvd bclg"),
                EnableSsl = true,
            };
        }

        public async Task SendVerificationEmailAsync(string email, string verificationCode)
        {
            email = email.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                throw new ArgumentException($"Invalid email address: '{email}'", nameof(email));

            var mailMessage = new MailMessage
            {
                From = new MailAddress("itcollegeschedule@gmail.com"),
                Subject = "Email Verification",
                Body = $"Your verification code is: {verificationCode}",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendPasswordAsync(string email, string Password)
        {
            email = email.Trim();
            var mailMessage = new MailMessage
            {
                From = new MailAddress("itcollegeschedule@gmail.com"),
                Subject = "Email Verification",
                Body = $"Your Password is: {Password}",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendForgetpassworodAsync(string email, string verificationCode)
        {
            email = email.Trim();
            var mailMessage = new MailMessage
            {
                From = new MailAddress("itcollegeschedule@gmail.com"),
                Subject = "For Get Password",
                Body = $"Your verification code is: {verificationCode}",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendStatus(string email, string body)
        {
            email = email.Trim();
            var mailMessage = new MailMessage
            {
                From = new MailAddress("itcollegeschedule@gmail.com"),
                Subject = "Respond to the joining request",
                Body = body,
                IsBodyHtml = false,
            };

            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);
        }

        

    }
}
