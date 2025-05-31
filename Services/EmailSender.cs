using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Thrift.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                _logger.LogInformation($"Attempting to send email to {email} with subject: {subject}");
                
                // Create mail message
                var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                
                mail.To.Add(new MailAddress(email));

                // Create smtp client
                var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    Port = _emailSettings.Port,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
                };

                _logger.LogInformation($"Connecting to SMTP server: {_emailSettings.SmtpServer}:{_emailSettings.Port}");
                _logger.LogInformation($"Using credentials for: {_emailSettings.Username}");
                
                // Send mail
                await smtpClient.SendMailAsync(mail);
                _logger.LogInformation("Email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email. Error: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                
                // For debugging purposes, don't throw the exception in development
                // This will allow the password reset flow to continue even if email fails
                #if DEBUG
                Console.WriteLine($"EMAIL WOULD HAVE BEEN SENT TO: {email}");
                Console.WriteLine($"SUBJECT: {subject}");
                Console.WriteLine($"BODY: {htmlMessage}");
                #else
                throw;
                #endif
            }
        }
    }
}