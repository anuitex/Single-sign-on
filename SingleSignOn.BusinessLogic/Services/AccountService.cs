using System;
using System.Text;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using SingleSignOn.Common;
using SingleSignOn.BusinessLogic.ResponseModels.Account;
using SingleSignOn.BusinessLogic.ViewModels.Account;
using SingleSignOn.Common;
using SingleSignOn.Configuration;
using SingleSignOn.DataAccess.Entities;
using SingleSignOn.DataAccess.Repositories;
using SingleSignOn.BusinessLogic.Interfaces;
using SingleSignOn.BusinessLogic.ViewModels.Account;
using SingleSignOn.BusinessLogic.ResponseModels.Account;
using Newtonsoft.Json;

namespace SingleSignOn.BusinessLogic.Services
{
    public class AccountService : IAccountService
    {
        private UserRepository<ApplicationUser> _userRepository;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly IEmailSender _emailSender;
        protected readonly IConfiguration _configuration;
        protected readonly SocialNetworksHelper _socialNetworksHelper;

        public AccountService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userRepository = new UserRepository<ApplicationUser>(_configuration);
            _userManager = userManager;
        }

        //public async Task<List<ApplicationUser>> GetAll()
        //{
        //    var userList = new List<ApplicationUser>();

        //    var users = await _userRepository.GetUsers();

        //    foreach (var user in users)
        //    {
        //        userList.Add(user);
        //    }
        //    return userList;
        //}

        public async Task<ApplicationUser> FindByName(string userEmail)
        {
            var user = await _userManager.FindByNameAsync(userEmail);
            return user;
        }

        public async Task<AccountLoginResponseModel> Login(LoginAccountViewModel model, string hostNameString)
        {
            SignInResult result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
            var userInfoViewModel = new UserInfoViewModel(user);
            var accountLoginResponseModel = new AccountLoginResponseModel();
            string returnUrl = null;

            if (!result.Succeeded)
            {
                return null;
            }

            if (result.RequiresTwoFactor)
            {
                string code = await _userManager.GenerateTwoFactorTokenAsync(user, "Default");
                await _emailSender.SendEmailAsync(user.Email, "Two-factor authentication", $"There is the code for login: <input value=\"{code}\"/>");

                userInfoViewModel.Token = string.Empty;
                returnUrl = $"/Account/LoginWith2fa?userId={user.Id}&returnUrl={model.ReturnUrl}";
            }

            if (!result.RequiresTwoFactor)
            {
                string token = GenerateJwtToken(model.Email, user);

                userInfoViewModel.Token = token;
                returnUrl = $"{_configuration["AuthCallback"]}?token={token}&returnUrl={model.ReturnUrl}";
            }

            accountLoginResponseModel = new AccountLoginResponseModel(userInfoViewModel, returnUrl);

            if (model.ReturnUrl.Contains(hostNameString))
            {
                accountLoginResponseModel.ReturnUrl = model.ReturnUrl;
                return accountLoginResponseModel;
            }

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

            if (result.IsLockedOut)
            {
                throw new Exception("Account has been locked!");
            }

            if (!result.Succeeded)
            {
                throw new Exception("Invalid authenticator code.");
            }

            var token = GenerateJwtToken(user.Email, user);
            var userInfoViewModel = new UserInfoViewModel(user, token);
            var returnUrl = $"{_configuration["AuthCallback"]}?token={token}&returnUrl={model.ReturnUrl}";
            var accountLoginResponseModel = new AccountLoginResponseModel(userInfoViewModel, returnUrl);
            return accountLoginResponseModel;
        }

        public async Task<IdentityResult> Register(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result;
        }

        public async Task SendForgotPassEmail(EmailViewModel model, string callbackUrl)
        {
            var emailConfig = new EmailConfiguration();
            var _emailProvider = new EmailProvider();
            var emailCredential = new EmailCredential();

            emailCredential.DisplayName = emailConfig.DisplayName;
            emailCredential.EmailDeliverySmptServer = emailConfig.EmailDeliverySmptServer;
            emailCredential.EmailDeliveryPort = emailConfig.EmailDeliveryPort;
            emailCredential.EmailDeliveryLogin = emailConfig.EmailDeliveryLogin;
            emailCredential.EmailDeliveryPassword = emailConfig.EmailDeliveryPassword;

            await _emailProvider.SendMessage(emailCredential, emailConfig.Subject, emailConfig.ForgotPasswordBodyStart + callbackUrl + emailConfig.ForgotPasswordBodyEnd, model.Email);
        }

        public async Task SendConfirmRegisterEmail(EmailViewModel model, string callbackUrl)
        {
            var emailConfig = new EmailConfiguration();
            var _emailProvider = new EmailProvider();
            var emailCredential = new EmailCredential();

            emailCredential.DisplayName = emailConfig.DisplayName;
            emailCredential.EmailDeliverySmptServer = emailConfig.EmailDeliverySmptServer;
            emailCredential.EmailDeliveryPort = emailConfig.EmailDeliveryPort;
            emailCredential.EmailDeliveryLogin = emailConfig.EmailDeliveryLogin;
            emailCredential.EmailDeliveryPassword = emailConfig.EmailDeliveryPassword;

            await _emailProvider.SendMessage(emailCredential, emailConfig.Subject, emailConfig.ConfirmAccountBodyStart + callbackUrl + emailConfig.ConfirmRegisterBodyEnd, model.Email);
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

        //
        // Summary:
        // Gets an URL of image used for user's avatar for Google account
        //
        // Parameters:
        //   profileId:
        //     The Goole profile identifier.
        public async Task<string> GetGoogleAvatar(string profileId)
        {
            string avatarUrl = string.Empty;
            var webClient = new HttpClient();
            var profileUrl = $"https://www.googleapis.com/plus/v1/people/{profileId}?fields=image&key={_configuration["GoogleApiKey"].ToString()}";

            dynamic response = JsonConvert.DeserializeObject<dynamic>(await webClient.GetStringAsync(profileUrl));
            bool isResponseImageUrlValid = ValidateGoogleAvatarResponseImageUrl(response);

            if (isResponseImageUrlValid == true)
            {
                var imageUrl = response.image.url.ToString();
                avatarUrl = imageUrl;

                if (imageUrl.IndexOf("?") != -1)
                {
                    avatarUrl = imageUrl.Substring(0, imageUrl.IndexOf("?"));
                }
            }
            
            return avatarUrl;
        }


        //
        // Summary:
        // Checks if an URL of an image used for user's avatar for Google is not null
        //
        // Parameters:
        //   response:
        //     Response to request for an object with image gotten from Google API.
        public bool ValidateGoogleAvatarResponseImageUrl (dynamic response)
        {
            if (response != null)
            {
                dynamic responseImageField = response.GetType().GetProperty("image");

                if (responseImageField != null && response.image != null)
                {
                    dynamic responseImageUrlField = responseImageField.GetProperty("url");

                    if (responseImageUrlField != null && response.image.url != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
