using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SSO.API.Common;
using SSO.API.Models;
using SSO.API.Models.AccountViewModels;
using SSO.API.Services.Interfaces;
using SSO.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (result.RequiresTwoFactor)
            {
                var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Default");
                await _emailSender.SendEmailAsync(user.Email, "Two-factor authentication", $"There is the code for login: <input value=\"{code}\"/>");

                return new AccountLoginResponseModel
                {
                    UserInfo = new UserInfoViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        token = String.Empty
                    },
                    ReturnUrl = $"/Account/LoginWith2fa?userId={user.Id}&returnUrl={model.ReturnUrl}"
                };
            }

            if (!result.Succeeded)
            {
                return null;
            }

            var token = GenerateJwtToken(model.Email, user);

            if (model.ReturnUrl.Contains(hostNameString))
            {
                return new AccountLoginResponseModel
                {
                    UserInfo = new UserInfoViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        token = token
                    },
                    ReturnUrl = model.ReturnUrl
                };
            }

            return new AccountLoginResponseModel
            {
                UserInfo = new UserInfoViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    token = token
                },
                ReturnUrl = $"{_configuration["AuthCallback"]}?token={token}&returnUrl={model.ReturnUrl}"
            };
        }

        public async Task<AccountLoginResponseModel> LoginWith2FA(LoginWith2faViewModel model)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new Exception($"Unable to load user with this ID.");
            }

            throw new NotImplementedException();
        }

        public Task<IActionResult> Register()
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> ForgotPassword()
        {
            throw new NotImplementedException();
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
