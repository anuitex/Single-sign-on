using Microsoft.AspNetCore.Identity;
using SingleSignOn.BusinessLogic.ViewModels.Account;
using SingleSignOn.DataAccess.Entities;
using System.Threading.Tasks;

namespace SingleSignOn.BusinessLogic.Interfaces
{
    public interface IAccountService
    {
        //Task<List<ApplicationUser>> GetAll();
        Task<IdentityResult> Register(ApplicationUser user, string password);
        Task<ApplicationUser> FindByName(string userEmail);
        Task SendForgotPasswordEmail(EmailViewModel model, string callbackUrl);
    }
}
