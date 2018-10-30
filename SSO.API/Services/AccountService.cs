using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SSO.API.Common;
using SSO.API.Models;
using SSO.DataAccess.Entities;
using SSO.API.Services.Interfaces;
using SSO.API.Models.AccountViewModels;

namespace SSO.API.Services
{
    public class AccountService : IAccountService
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly IEmailSender _emailSender;
        protected readonly IConfiguration _configuration;
        protected readonly SocialNetworksHelper _socialNetworksHelper;

        public AccountService(
            SocialNetworksHelper socialNetworksHelper,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _socialNetworksHelper = socialNetworksHelper;
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<AccountLoginResponseModel> Login(LoginAccountView model, string hostNameString)
        {
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
            var accountLoginResponseModel = new AccountLoginResponseModel();
            var userInfoViewModel = new UserInfoViewModel(user);

            if (result.RequiresTwoFactor)
            {
                var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Default");
                await _emailSender.SendEmailAsync(user.Email, "Two-factor authentication", $"There is the code for login: <input value=\"{code}\"/>");

                userInfoViewModel.Token = string.Empty;
                accountLoginResponseModel.UserInfo = userInfoViewModel;

                if (model.ReturnUrl.Contains(hostNameString))
                {
                    accountLoginResponseModel.ReturnUrl = model.ReturnUrl;
                    return accountLoginResponseModel;
                }

                accountLoginResponseModel.ReturnUrl = $"/Account/LoginWith2fa?userId={user.Id}&returnUrl={model.ReturnUrl}";
                return accountLoginResponseModel;
            }

            if (!result.Succeeded)
            {
                return null;
            }

            var token = GenerateJwtToken(model.Email, user);

            userInfoViewModel.Token = token;
            accountLoginResponseModel.UserInfo = userInfoViewModel;

            if (model.ReturnUrl.Contains(hostNameString))
            {
                accountLoginResponseModel.ReturnUrl = model.ReturnUrl;
                return accountLoginResponseModel;
            }
            
            accountLoginResponseModel.ReturnUrl = $"{_configuration["AuthCallback"]}?token={token}&returnUrl={model.ReturnUrl}";
            return accountLoginResponseModel;
        }

        public async Task<AccountLoginResponseModel> LoginWith2FA(LoginWith2faViewModel model)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new Exception($"Unable to load user with this ID.");
            }

            var result = await _signInManager.TwoFactorSignInAsync("Default", model.TwoFactorCode, true, model.RememberMachine);

            if(result.IsLockedOut)
            {
                throw new Exception("Account has been locked!");
            }

            if(!result.Succeeded)
            {
                throw new Exception("Invalid authenticator code.");
            }

            var accountLoginResponseModel = new AccountLoginResponseModel();
            var userInfoViewModel = new UserInfoViewModel(user);
            var token = GenerateJwtToken(user.Email, user);
            userInfoViewModel.Token = token;
            accountLoginResponseModel.UserInfo = userInfoViewModel;
            accountLoginResponseModel.ReturnUrl = $"{_configuration["AuthCallback"]}?token={token}&returnUrl={model.ReturnUrl}";
            return accountLoginResponseModel;
        }

        public async Task<string> Register(RegisterAccountView model, IUrlHelper url, HttpRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            if (string.IsNullOrEmpty(model.ReturnUrl))
            {
                model.ReturnUrl = _configuration["RedirectUrl"];
            }
            
            var result = await _userManager.CreateAsync(user, model.Password);
            var error = GetErrors(result).Select(x => x.Description).FirstOrDefault();

            if(error != null)
            {
                throw new Exception(error);
            }

            if (!result.Succeeded)
            {
                return null;
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = url.EmailConfirmationLink(user.Id, code, request.Scheme);
            await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

            return model.ReturnUrl;
        }

        public async Task<bool> ForgotPassword(ForgotPasswordViewModel model, IUrlHelper url, HttpRequest request)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return false;
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = url.ResetPasswordCallbackLink(user.Id, code, request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Reset Password", $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

            return true;
        }

        public Task<IActionResult> GetUser()
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GoogleToken()
        {
            throw new NotImplementedException();
        }

        public List<IdentityError> GetErrors(IdentityResult result)
        {
            var errors = new List<IdentityError>();

            foreach (var error in result.Errors)
            {
                errors.Add(error);
            }

            return errors;
        }

        public string GenerateJwtToken(string email, ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));
            var appSettings = _configuration.GetSection("AppSettings");
            var authTokenProviderOptions = _configuration.GetSection("AuthTokenProviderOptions");
            var key = appSettings?["JwtKey"] ?? "default_secret_key";
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var validIssuer = authTokenProviderOptions?["Issuer"];

            var token = new JwtSecurityToken(
                validIssuer,
                validIssuer,
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
