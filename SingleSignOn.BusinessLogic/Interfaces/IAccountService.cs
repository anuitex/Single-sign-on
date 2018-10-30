using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Identity;
using SingleSignOn.DataAccess.Entities;
using SingleSignOn.ViewModels.Account;
using System.Threading.Tasks;
using System.Web;

namespace SingleSignOn.BusinessLogic.Interfaces
{
    public interface IAccountService
    {
        //Task<List<ApplicationUser>> GetAll();
        Task<IdentityResult> Register(ApplicationUser user, string password);
        Task<ApplicationUser> FindByName(string userEmail);
        Task SendEmail(ForgotPasswordViewModel model, string callbackUrl);
    }
}
