using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSO.API.Models;
using SSO.API.Models.AccountViewModels;
using SSO.API.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using SSO.DataAccess.Entities;

namespace SSO.API.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           IEmailSender emailSender,
           IConfiguration configuration)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            var view = new LoginAccountView();
            if (String.IsNullOrEmpty(returnUrl))
            {
                view.ReturnUrl = _configuration["RedirectUrl"];
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(view);
        }

        [HttpGet]
        public IActionResult Register(string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl))
            {
                returnUrl = _configuration["RedirectUrl"];
            }
            var view = new RegisterAccountView();
            view.ReturnUrl = returnUrl;
            return View(view);
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
            if (result.Succeeded)
            {
                return View();
            }
            return Redirect("/Error/Index");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }
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
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
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

        [HttpGet]
        public async Task<IActionResult> LoginWith2fa(int userId = -1, string returnUrl = null)
        {
            if (userId == -1)
            {
                return Redirect("/Error/Index");
            }
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            var model = new LoginWith2faViewModel();
            model.UserId = userId;
            model.ReturnUrl = returnUrl;
            return View(model);
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        //{
        //    // Ensure the user has gone through the username & password screen first
        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load two-factor authentication user.");
        //    }

        //    ViewData["ReturnUrl"] = returnUrl;

        //    return View();
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
        //        return Redirect(returnUrl);
        //    }
        //    if (result.IsLockedOut)
        //    {
        //        return RedirectToAction(nameof(Lockout));
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
        //        return View();
        //    }
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public IActionResult ExternalLogin(string provider, string returnUrl = null)
        //{
        //    // Request a redirect to the external login provider.
        //    var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        //    var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        //    return Challenge(properties, provider);
        //}

        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        //{
        //    if (remoteError != null)
        //    {
        //        //ErrorMessage = $"Error from external provider: {remoteError}";
        //        return RedirectToAction(nameof(Login));
        //    }
        //    var info = await _signInManager.GetExternalLoginInfoAsync();
        //    if (info == null)
        //    {
        //        return RedirectToAction(nameof(Login));
        //    }

        //    // Sign in the user with this external login provider if the user already has a login.
        //    var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        //    if (result.Succeeded)
        //    {
        //        return Redirect(returnUrl);
        //    }
        //    if (result.IsLockedOut)
        //    {
        //        return RedirectToAction(nameof(Lockout));
        //    }
        //    else
        //    {
        //        // If the user does not have an account, then ask the user to create an account.
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
        //        // Get the information about the user from the external login provider
        //        var info = await _signInManager.GetExternalLoginInfoAsync();
        //        if (info == null)
        //        {
        //            throw new ApplicationException("Error loading external login information during confirmation.");
        //        }
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await _userManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            result = await _userManager.AddLoginAsync(user, info);
        //            if (result.Succeeded)
        //            {
        //                await _signInManager.SignInAsync(user, isPersistent: false);
        //                return Redirect(returnUrl);
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    ViewData["ReturnUrl"] = returnUrl;
        //    return View(nameof(ExternalLogin), model);
        //}

        //[HttpGet]
        //public IActionResult AccessDenied()
        //{
        //    return View();
        //}




    }
}
