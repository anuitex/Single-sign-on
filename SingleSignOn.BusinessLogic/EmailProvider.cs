using SingleSignOn.Entities;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SingleSignOn.BusinessLogic
{
    public class EmailProvider
    {
        public async Task SendMessage(EmailCredential emailCredential, string subject, string body, string toEmail)
        {
            var fromAddress = new MailAddress(emailCredential.EmailDeliveryLogin, emailCredential.DisplayName);
            var toAddress = new MailAddress(toEmail, toEmail);

            var smtp = new SmtpClient
            {
                Host = emailCredential.EmailDeliverySmptServer,
                Port = emailCredential.EmailDeliveryPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailCredential.EmailDeliveryLogin, emailCredential.EmailDeliveryPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body })
            {
                await smtp.SendMailAsync(message);
            }
        }
    }
}
