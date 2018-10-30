using System.Threading.Tasks;

namespace SingleSignOn.WebTest.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
