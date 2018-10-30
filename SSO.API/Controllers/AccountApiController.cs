﻿using Microsoft.AspNetCore.Authorization;
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
            var userProfileViewModel = await _accountService.GoogleToken(token);

            var loginResult = await LoginExternal(userProfileViewModel, "google");

            if (loginResult.GetType() != typeof(OkObjectResult))
            {
                return loginResult;
            }

            return await GetLoginResponse("google", userProfileViewModel.Email);
        }

        private async Task<IActionResult> GetLoginResponse(string provider, string email)
        {
            try
            {
                var result = await _accountService.GetLoginResponse(provider, email);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<IActionResult> LoginExternal(AuthenticationViewModel model, string provider)
        {
            var user = await _userManager.FindByEmailAsync(model.Email) ?? await _userManager.FindByLoginAsync(provider, model.Email);

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
    }
}