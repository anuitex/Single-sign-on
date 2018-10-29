using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SSO.API.Models;
using SSO.API.Models.AccountViewModels;
using SSO.DataAccess.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSO.API.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountLoginResponseModel> Login(LoginAccountView model, string hostNameString);
        Task<AccountLoginResponseModel> LoginWith2FA(LoginWith2faViewModel model);
        Task<IActionResult> Register();
        Task<IActionResult> GetUser();
        Task<IActionResult> ForgotPassword();
        Task<IActionResult> GoogleToken();
        List<IdentityError> GetErrors(IdentityResult result);
        string GenerateJwtToken(string email, ApplicationUser user);
    }
}
