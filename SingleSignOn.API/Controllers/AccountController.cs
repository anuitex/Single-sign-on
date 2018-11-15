using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SingleSignOn.BusinessLogic.Interfaces;
using SingleSignOn.BusinessLogic.Services;
using SingleSignOn.BusinessLogic.ViewModels.Account;
using SingleSignOn.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SingleSignOn.API.Controllers
{
    public class AccountController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly IConfiguration _configuration;
        private IAccountService _accountService;
        private readonly ILogger _logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger,
                                 IAccountService accountService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginAccountViewModel model)
        {
            if (string.IsNullOrEmpty(model.ReturnUrl))
            {
                model.ReturnUrl = _configuration["RedirectUrl"];
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return View(model);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(EmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
            await _accountService.SendForgotPasswordEmail(model, callbackUrl);

            return RedirectToAction("ForgotPasswordConfirmation", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            return RedirectToAction(nameof(AccountController.Login));
        }

        [HttpGet]
        public IActionResult Register(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = _configuration["RedirectUrl"];
            }

            var view = new RegisterAccountViewModel();
            view.ReturnUrl = returnUrl;

            return View(view);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterAccountViewModel model)
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

            var _accountService = new AccountService(_configuration, _userManager);
            var existsUser = await _accountService.FindByName(model.Email);

            if (existsUser != null)
            {
                return RedirectToAction("ExistsUser", "Account", model);
            }

            var result = await _accountService.Register(user, model.Password, model.ReturnUrl);
            //var error = GetErrors(result).Select(x => x.Description).FirstOrDefault();
            
            if (result != null)
            {
                return BadRequest();
            }

            _logger.LogInformation("User created a new account with password.");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
            await _accountService.SendConfirmRegisterEmail(new EmailViewModel() { Email = model.Email }, callbackUrl);
            //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User created a new account with password.");

            return RedirectToAction("Index", "Home");
        }

        public ActionResult ExistsUser(RegisterAccountViewModel model)
        {
            if (model != null)
            {
                ModelState.AddModelError("UserAlreadyExists", "This user already exists.");

                return View("Register", model);
            }

            return View(new RegisterAccountViewModel());
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [HttpGet]
        public ActionResult Success()
        {
            return View();
        }

        private string GenerateJwtToken(string email, ApplicationUser user)
        {
            try
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

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = expires
                    });

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
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

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return Redirect("/Error/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Redirect("/Error/Index");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded)
            {
                return Redirect("/Error/Index");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                return Redirect("/Error/Index");
            }

            var model = new ResetPasswordViewModel { Code = code };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Redirect("/Error/Index");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            AddErrors(result);

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}

