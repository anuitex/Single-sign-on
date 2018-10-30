using System.Threading.Tasks;

namespace SingleSignOn.BusinessLogic.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}