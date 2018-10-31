using SingleSignOn.DataAccess.Entities;
using System;
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
                UseDefaultCredentials = false,
                Host = emailCredential.EmailDeliverySmptServer,
                Port = emailCredential.EmailDeliveryPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(emailCredential.EmailDeliveryLogin, emailCredential.EmailDeliveryPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress))
            {
                message.Subject = subject;
                message.Body = body;

                try
                {
                    await smtp.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message, ex.Data);
                }
            }
        }
    }
}
