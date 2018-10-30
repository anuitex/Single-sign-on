using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SSO.API.Models;
using SSO.API.Models.AccountViewModels;
using SSO.DataAccess.Entities;
using SSO.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSO.API.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountLoginResponseModel> Login(LoginAccountView model, string hostNameString);
        Task<AccountLoginResponseModel> LoginWith2FA(LoginWith2faViewModel model);
        Task<string> Register(RegisterAccountView model, IUrlHelper url, HttpRequest request);
        Task<ApplicationUser> GetUser(string name);
        Task<bool> ForgotPassword(ForgotPasswordViewModel model, IUrlHelper url, HttpRequest request);
        Task<AuthenticationViewModel> GoogleToken(string token);
        List<IdentityError> GetErrors(IdentityResult result);
        string GenerateJwtToken(string email, ApplicationUser user);
        Task<AccountLoginResponseModel> GetLoginResponse(string provider, string email);
        Task<Microsoft.AspNetCore.Identity.SignInResult> LoginExternal(AuthenticationViewModel model, string provider);
    }
}
