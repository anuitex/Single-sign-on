using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SingleSignOn.BusinessLogic.Interfaces;
using SingleSignOn.BusinessLogic.ResponseModels.Account;
using SingleSignOn.BusinessLogic.Services;
using SingleSignOn.ViewModels.Account;
using SingleSignOn.DataAccess.Entities;
using SingleSignOn.DataAccess.Providers;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Security;

namespace SingleSignOn.WebTest.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private IAccountService _accountService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAccountService accountService,
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _accountService = accountService;
            _configuration = configuration;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            //await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                AccountResponseModel apiResponse = ApiProvider.PostSync<LoginViewModel, AccountResponseModel>("AccountApi/Login", model);


                //var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (apiResponse.IsOk)
                {
                    if (!string.IsNullOrEmpty(apiResponse.UserInfo.Token))
                    {
                        Response.Cookies.Append("Token", apiResponse.UserInfo.Token);
                        _logger.LogInformation("User logged in.");
                        return RedirectToLocal(returnUrl);
                    }

                    return View(model);
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                //}
                //if (result.IsLockedOut)
                //{
                //    _logger.LogWarning("User account locked out.");
                //    return RedirectToAction(nameof(Lockout));
                //}
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                    return View(model);
                }
            }

            return View(model);
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

            if (ModelState.IsValid)
            {
                AccountResponseModel responseUser = ApiProvider.PostSync<RegisterAccountViewModel, AccountResponseModel>("AccountApi/Register", model);

                if (responseUser.IsOk)
                {

                    _logger.LogInformation("User created a new account with password.");

                    if (!string.IsNullOrEmpty(responseUser.UserInfo.Token))
                    {
                        Response.Cookies.Append("Token", responseUser.UserInfo.Token);
                        return RedirectToAction("Index", "Home", new { area = "Home" });
                    }

                    //await _signInManager.SignInAsync(responseUser, isPersistent: false);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                return View(model);
            }
            return BadRequest("Invalid user");
        }

        public async Task<IActionResult> Logout()
        {
            //await _signInManager.SignOutAsync();

            Response.Cookies.Delete("Token");

            _logger.LogInformation("User logged out.");

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(EmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null /*|| !(await _userManager.IsEmailConfirmedAsync(user))*/)
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, Request.Scheme);
                await _accountService.SendForgotPasswordEmail(model, callbackUrl);

                return RedirectToAction("ForgotPasswordConfirmation", "Account");

                //var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                //var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, Request.Scheme);
                //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                //   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                //return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }

            var model = new ResetPasswordViewModel { Code = code };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            AddErrors(result);

            return View();
        }


        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(returnUrl);
            }
            else
            {
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

                Random rnd = new Random();
                var password = Membership.GeneratePassword(12, 2) + rnd.Next(0, 99).ToString();

                AccountService _accountService = new AccountService(_configuration, _userManager);

                var existsUser = await _accountService.FindByName(model.Email);

                if (existsUser != null)
                {
                    return View("Error");
                }
                try
                {
                    //var result = await _accountService.Register(user, password);
                    ApplicationUser responseUser = ApiProvider.PostSync<RegisterAccountViewModel, ApplicationUser>("AccountApi/Register", new RegisterAccountViewModel() { Email = user.Email, Password = password, ConfirmPassword = password });

                    if (responseUser != null)
                    {
                        var result = await _userManager.AddLoginAsync(user, info);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    return View(nameof(ExternalLogin), model);
                }

            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        //{
        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load two-factor authentication user.");
        //    }

        //    var model = new LoginWith2faViewModel { RememberMe = rememberMe };
        //    ViewData["ReturnUrl"] = returnUrl;

        //    return View(model);
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        //    }

        //    var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        //    var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

        //    if (result.Succeeded)
        //    {
        //        _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
        //        return RedirectToLocal(returnUrl);
        //    }
        //    else if (result.IsLockedOut)
        //    {
        //        _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
        //        return RedirectToAction("LockedOut");
        //    }
        //    else
        //    {
        //        _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
        //        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        //        return View();
        //    }
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load two-factor authentication user.");
        //    }

        //    var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

        //    var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        //    if (result.Succeeded)
        //    {
        //        _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
        //        return RedirectToLocal(returnUrl);
        //    }
        //    if (result.IsLockedOut)
        //    {
        //        _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
        //        return RedirectToAction(returnUrl);
        //    }
        //    else
        //    {
        //        _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
        //        ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
        //        return View();
        //    }
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public IActionResult ExternalLogin(string provider, string returnUrl = null)
        //{
        //    string redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        //    AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        //    ChallengeResult challange = Challenge(properties, provider);
        //    return challange;
        //}

        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        //{
        //    if (remoteError != null)
        //    {
        //        ErrorMessage = $"Error from external provider: {remoteError}";
        //        return RedirectToAction(nameof(Login));
        //    }

        //    var info = await _signInManager.GetExternalLoginInfoAsync();

        //    if (info == null)
        //    {
        //        return RedirectToAction(nameof(Login));
        //    }

        //    var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        //    if (result.Succeeded)
        //    {
        //        _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
        //        return RedirectToLocal(returnUrl);
        //    }
        //    if (result.IsLockedOut)
        //    {
        //        return RedirectToAction(returnUrl);
        //    }
        //    else
        //    {
        //        ViewData["ReturnUrl"] = returnUrl;
        //        ViewData["LoginProvider"] = info.LoginProvider;
        //        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        //        return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
        //    }
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var info = await _signInManager.GetExternalLoginInfoAsync();
        //        if (info == null)
        //        {
        //            throw new ApplicationException("Error loading external login information during confirmation.");
        //        }
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email};
        //        var rnd = new Random();
        //        var password = Membership.GeneratePassword(12,3)+ rnd.Next(0,99).ToString();
        //        AccountService _accountService = new AccountService(_configuration, _userManager);

        //        var existsUser = await _accountService.FindByName(model.Email);

        //        if (existsUser != null)
        //        {
        //            return View("Error");
        //        }

        //        var result = await _accountService.Register(user, password);

        //        //var result = await _userManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            result = await _userManager.AddLoginAsync(user, info);
        //            if (result.Succeeded)
        //            {
        //                await _signInManager.SignInAsync(user, isPersistent: false);
        //                _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
        //                return RedirectToLocal(returnUrl);
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    ViewData["ReturnUrl"] = returnUrl;
        //    return View(nameof(ExternalLogin), model);
        //}

    }
}