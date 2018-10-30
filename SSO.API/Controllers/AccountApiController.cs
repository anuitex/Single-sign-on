using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SSO.API.Common;
using SSO.API.Models;
using SSO.API.Models.AccountViewModels;
using SSO.API.Services;
using SSO.API.Services.Interfaces;
using SSO.DataAccess.Entities;
using SSO.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SSO.API.Controllers
{
    public class AccountApiController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly IEmailSender _emailSender;
        protected readonly IConfiguration _configuration;
        protected readonly SocialNetworksHelper _socialNetworksHelper;
        protected readonly IAccountService _accountService;

        public AccountApiController(SocialNetworksHelper socialNetworksHelper, UserManager<ApplicationUser> userManager,
                                    SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IConfiguration configuration)
        {
            _accountService = new AccountService(socialNetworksHelper, userManager, signInManager, emailSender, configuration);
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
            if (string.IsNullOrEmpty(model.ReturnUrl))
            {
                model.ReturnUrl = _configuration["RedirectUrl"];
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model!");
            }

            var result = await _accountService.Login(model, Request.Host.ToString());

            if (result == null)
            {
                return BadRequest("Invalid login attempt!");
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("api/account/loginWith2fa")]
        public async Task<IActionResult> LoginWith2fa([FromBody]LoginWith2faViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var serviceResult = await _accountService.LoginWith2FA(model);
                return Ok(serviceResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/account/register")]
        public async Task<IActionResult> Register([FromBody]RegisterAccountView model)
        {
            try
            {
                var serviceResult = await _accountService.Register(model, Url, Request);
                return Ok(serviceResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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

            var result = await _accountService.ForgotPassword(model, Url, Request);

            if (!result)
            {
                return BadRequest("User doesn't exist or isn't confirmed");
            }

            return Ok();
        }

        [HttpGet]
        [Route("google-token/{token}")]
        public async Task<IActionResult> GoogleToken(string token)
        {
            string google = "google";

            try
            {
                var userProfileViewModel = await _accountService.GoogleToken(token);

                var loginResult = LoginExternal(userProfileViewModel, google);//cant be done

                var result = await _accountService.GetLoginResponse(google, userProfileViewModel.Email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<IActionResult> LoginExternal(AuthenticationViewModel model, string provider)//cant be done
        {
            var res = await _accountService.LoginExternal(model, provider);
            
            var loginResult = await _signInManager.ExternalLoginSignInAsync(provider, model.Email, false);

            return ProcessExternalLoginResult(loginResult, model.Email);//cant be done
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
    }
}