using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using EmployeeBOApp.Models;

namespace EmployeeBOApp.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }
        public async Task SendEmailAsync(IEnumerable<string> toEmails, string subject, string body, bool isHtml = false, IEnumerable<string>? ccEmails = null)
        {
            using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port))
            {
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                // Add TO recipients
                foreach (var toEmail in toEmails)
                {
                    mailMessage.To.Add(toEmail);
                }

                // Add CC recipients if any
                if (ccEmails != null)
                {
                    foreach (var ccEmail in ccEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                    {
                        mailMessage.CC.Add(ccEmail);
                    }
                }
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}