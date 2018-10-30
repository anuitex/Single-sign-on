﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SSO.API.Common;
using SSO.API.Models;
using SSO.API.Models.AccountViewModels;
using SSO.API.Services.Interfaces;
using SSO.DataAccess.Entities;
using SSO.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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

            var result = await _signInManager.TwoFactorSignInAsync("Default", model.TwoFactorCode, true, model.RememberMachine);

            if(result.IsLockedOut)
            {
                throw new Exception("Account has been locked!");
            }

            if(!result.Succeeded)
            {
                throw new Exception("Invalid authenticator code.");
            }

            var token = GenerateJwtToken(user.Email, user);

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

        public async Task<string> Register(RegisterAccountView model, IUrlHelper url, HttpRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            if (String.IsNullOrEmpty(model.ReturnUrl))
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

        public async Task<ApplicationUser> GetUser(string name)
        {
            var appUser = await _userManager.FindByEmailAsync(name);
            return appUser;
        }

        public async Task<AuthenticationViewModel> GoogleToken(string token)
        {
            var profileResponse = await _socialNetworksHelper.GetGoogleDetailsByToken(token);
            var email = profileResponse.emails[0].value;
            var userName = string.IsNullOrWhiteSpace(profileResponse.displayName.ToString())
               ? email
               : profileResponse.displayName.ToString();

            var userProfileViewModel = new AuthenticationViewModel { Email = email, GoogleProfileId = profileResponse.id.ToString() };

            if (profileResponse.name != null)
            {
                if (profileResponse.name.givenName != null)
                {
                    userProfileViewModel.FirstName = profileResponse.name.givenName;
                }

                if (profileResponse.name.familyName != null)
                {
                    userProfileViewModel.LastName = profileResponse.name.familyName;
                }
            }

            if (profileResponse.image != null && profileResponse.image.url != null)
            {
                var imageUrl = profileResponse.image.url.ToString();
                userProfileViewModel.PhotoUrl = imageUrl.IndexOf("?") != -1
                    ? imageUrl.Substring(0, imageUrl.IndexOf("?"))
                    : imageUrl;
            }

            return userProfileViewModel;
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

        public async Task<AccountLoginResponseModel> GetLoginResponse(string provider, string email)
        {
            var user = await _userManager.FindByLoginAsync(provider, email);

            if (user == null)
            {
                throw new Exception("User login info not found");
            }

            var token = GenerateJwtToken(email, user);

            if (!string.IsNullOrEmpty(token))
            {
                return new AccountLoginResponseModel
                {
                    UserInfo = new UserInfoViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        token = token
                    },
                    ReturnUrl = $"{_configuration["AuthCallback"]}?token={token}&returnUrl={_configuration["RedirectUrl"]}"
                };
            }

            throw new Exception("Cannot verify the login. Probably user profile is disabled or deleted.");
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
