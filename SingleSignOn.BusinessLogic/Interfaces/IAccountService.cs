using Microsoft.AspNetCore.Identity;
using SingleSignOn.BusinessLogic.ResponseModels.Account;
using SingleSignOn.ViewModels.Account;
using SingleSignOn.DataAccess.Entities;
using System.Threading.Tasks;

namespace SingleSignOn.BusinessLogic.Interfaces
{
    public interface IAccountService
    {
        //Task<List<ApplicationUser>> GetAll();
        Task<AccountResponseModel> Register(ApplicationUser user, string password, string returnUrl);
        Task<ApplicationUser> FindByName(string userEmail);
        Task SendForgotPasswordEmail(EmailViewModel model, string callbackUrl);
        Task<AccountResponseModel> Login(LoginAccountViewModel model, string hostNameString);
        Task SendConfirmRegisterEmail(EmailViewModel email, string callBackUrl);
    }
}
