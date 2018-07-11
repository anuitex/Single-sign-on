using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SSO.API.Common;
using SSO.API.Models;
using SSO.API.Models.AccountViewModels;
using SSO.API.Services;
using SSO.DataAccess.Entities;
using SSO.ViewModels;

namespace SSO.API.Controllers
{
    public class AccountApiController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly IEmailSender _emailSender;
        protected readonly IConfiguration _configuration;
        protected readonly SocialNetworksHelper _socialNetworksHelper;
        public AccountApiController(
            SocialNetworksHelper socialNetworksHelper,
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           IEmailSender emailSender,
           IConfiguration configuration
           )
        {
            _socialNetworksHelper = socialNetworksHelper;
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("api/account/login")]
        public async Task<IActionResult> Login([FromBody]LoginAccountView model)
        {
            if (String.IsNullOrEmpty(model.ReturnUrl))
            {
                model.ReturnUrl = _configuration["RedirectUrl"];
            }
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model!");
            }
            
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (result.RequiresTwoFactor)
            {
               
                var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Default");
                await _emailSender.SendEmailAsync(user.Email, "Two-factor authentication",
                    $"There is the code for login: <input value=\"{code}\"/>");
                return Ok(new
                {
                    UserInfo = new
                    {
                        user.Id,
                        user.UserName,
                        token = String.Empty
                    },
                    ReturnUrl = $"/Account/LoginWith2fa?userId={user.Id}&returnUrl={model.ReturnUrl}"
                });
            }
            if (!result.Succeeded)
            {
                return BadRequest("Invalid login attempt!");
            }
            var token = GenerateJwtToken(model.Email, user);
            if (model.ReturnUrl.Contains(Request.Host.ToString()))
            {
                return Ok(new
                {
                    UserInfo = new
                    {
                        user.Id,
                        user.UserName,
                        token
                    },
                    model.ReturnUrl
                });
            }
            return Ok(new
            {
                UserInfo = new
                {
                    user.Id,
                    user.UserName,
                    token
                },
                ReturnUrl = $"{_configuration["AuthCallback"]}?token={token}&returnUrl={model.ReturnUrl}"
            });
        }

        [HttpPost]
        [Route("api/account/loginWith2fa")]
        public async Task<IActionResult> LoginWith2fa([FromBody]LoginWith2faViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return BadRequest($"Unable to load user with this ID.");
            }
            
            var result = await _signInManager.TwoFactorSignInAsync("Default",model.TwoFactorCode, true, model.RememberMachine);

            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user.Email, user);
                return Ok(new
                {
                    UserInfo = new
                    {
                        user.Id,
                        user.UserName,
                        token
                    },
                    ReturnUrl = $"{_configuration["AuthCallback"]}?token={token}&returnUrl={model.ReturnUrl}"
                });
            }
            if (result.IsLockedOut)
            {
                return BadRequest("Account has been locked!");
            }
            return BadRequest("Invalid authenticator code.");
        }


        [HttpPost]
        [Route("api/account/register")]
        public async Task<IActionResult> Register([FromBody]RegisterAccountView model)
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
            if (error != null)
            {
                return BadRequest(error);
            }
            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);
                return Ok(model.ReturnUrl);
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        [Route("api/account/getUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var name = User.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;
                var appUser = await _userManager.FindByEmailAsync(name);
                return Ok(appUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/account/forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model!");
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return BadRequest("User doesn't exist or isn't confirmed");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Reset Password",
               $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
            return Ok();
        }

        [HttpGet]
        [Route("google-token/{token}")]
        public async Task<IActionResult> GoogleToken(string token)
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

            var loginResult = await LoginExternal(userProfileViewModel, "google");

            if (loginResult.GetType() != typeof(OkObjectResult))
            {
                return loginResult;
            }

            return await GetLoginResponse("google", email.ToString());
        }

        private async Task<IActionResult> GetLoginResponse(string provider, string email)
        {
            var user = await _userManager.FindByLoginAsync(provider, email);
            if (user == null)
            {
                return BadRequest("User login info not found");
            }
            var token = GenerateJwtToken(email, user);

            if (!String.IsNullOrEmpty(token))
            {
                return Ok(new
                {
                    UserInfo = new
                    {
                        user.Id,
                        user.UserName,
                        token
                    },
                    ReturnUrl = $"{_configuration["AuthCallback"]}?token={token}&returnUrl={_configuration["RedirectUrl"]}"
                });
            }

            return BadRequest("Cannot verify the login. Probably user profile is disabled or deleted.");
        }

        private async Task<IActionResult> LoginExternal(AuthenticationViewModel model, string provider)
        {
            var user = await _userManager.FindByEmailAsync(model.Email) ??
                       await _userManager.FindByLoginAsync(provider, model.Email);

            if (user == null)
            {
                var newUser = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhotoUrl = model.PhotoUrl,
                    AvatarType = model.AvatarType,
                    EmailConfirmed = true,
                    FacebookProfileId = model.FacebookProfileId,
                    GoogleProfileId = model.GoogleProfileId,
                    VkProfileId = model.VkProfileId,
                    TwitterProfileId = model.TwitterProfileId,
                    RegistrationDate = DateTime.Now,
                    AvatarSet = !String.IsNullOrWhiteSpace(model.PhotoUrl)
                };

                IdentityResult result = await CreateNewUser(newUser, String.Empty, provider);

                if (!result.Succeeded)
                {
                    return BadRequest("Error creating user");
                }

             }

            var loginResult = await _signInManager.ExternalLoginSignInAsync(provider, model.Email, false);

            return ProcessExternalLoginResult(loginResult, model.Email);
        }

        //TODO: figure out proper logic for processing loginResults
        private IActionResult ProcessExternalLoginResult(Microsoft.AspNetCore.Identity.SignInResult loginResult, string email)
        {
            return Ok(new { loginResult, email });
        }

        //TODO: figure out proper logic for creating user
        private async Task<IdentityResult> CreateNewUser(ApplicationUser newUser, string param, string provider)
        {
            var result = await _userManager.CreateAsync(newUser);
            return result;
        }

        private string GenerateJwtToken(string email, ApplicationUser user)
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

        private List<IdentityError> GetErrors(IdentityResult result)
        {
            var errors = new List<IdentityError>();
            foreach (var error in result.Errors)
            {
                errors.Add(error);
            }
            return errors;
        }
    }
}