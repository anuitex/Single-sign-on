using Microsoft.Extensions.Configuration;
using SSO.API.Services.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SSO.API.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var smpt = _configuration["EmailSmtp"];
            var port = int.Parse(_configuration["EmailPort"]);
            var from = _configuration["EmailAddress"];
            var password = _configuration["EmailPassword"];
            var client = new SmtpClient(smpt, port);

            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(from, password);
            client.EnableSsl = true;
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(from);
            mailMessage.IsBodyHtml = true;
            mailMessage.To.Add(email);
            mailMessage.Body = message;
            mailMessage.Subject = subject;

            return client.SendMailAsync(mailMessage);
        }
    }
}
