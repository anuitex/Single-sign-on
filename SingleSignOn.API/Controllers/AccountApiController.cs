﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SingleSignOn.DataAccess.Entities;
using SingleSignOn.BusinessLogic.Interfaces;
using SingleSignOn.BusinessLogic.Services;
using SingleSignOn.BusinessLogic.ViewModels.Account;

namespace SingleSignOn.API.Controllers
{
    [Route("[controller]")]
    public class AccountApiController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly IEmailSender _emailSender;
        protected readonly IConfiguration _configuration;
        //protected readonly SocialNetworksHelper _socialNetworksHelper;
        protected readonly IAccountService _accountService;

        public AccountApiController(/*SocialNetworksHelper socialNetworksHelper,*/ UserManager<ApplicationUser> userManager,
                                    SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IConfiguration configuration)
        {
            _accountService = new AccountService(/*socialNetworksHelper,*/configuration, userManager/*, signInManager, emailSender*/ );
            //_socialNetworksHelper = socialNetworksHelper;
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody]LoginAccountViewModel model)
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

        //[HttpPost]
        //[Route("api/account/loginWith2fa")]
        //public async Task<IActionResult> LoginWith2fa([FromBody]LoginWith2faViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    try
        //    {
        //        var serviceResult = await _accountService.LoginWith2FA(model);
        //        return Ok(serviceResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody]RegisterAccountViewModel model)
        {
            try
            {
                //var user = new ApplicationUser
                //{
                //    UserName = model.Email,
                //    Email = model.Email
                //};
                //if (String.IsNullOrEmpty(model.ReturnUrl))
                //{
                //    model.ReturnUrl = _configuration["RedirectUrl"];
                //}

                //AccountService _accountService = new AccountService(_configuration, _userManager);

                //var existsUser = await _accountService.FindByName(model.Email);

                //if (existsUser != null)
                //{
                //    return RedirectToAction("ExistsUser", "Account", model);
                //}

                //var result = await _accountService.Register(user, model.Password);

                //if (result.Succeeded != true)
                //{
                //    return BadRequest("User invalid");
                //}
                //if (result.Succeeded)
                //{
                //    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //    //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                //    //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                //    await _signInManager.SignInAsync(user, isPersistent: false);
                  

                //    return RedirectToAction("Index", "Home", new { area = "Home" });
                //}

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        [Route("GetUser")]
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

        //[HttpPost]
        //[Route("api/account/forgotPassword")]
        //public async Task<IActionResult> ForgotPassword([FromBody]EmailViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest("Invalid model!");
        //    }

        //    var result = await _accountService.ForgotPassword(model, Url, Request);

        //    if (!result)
        //    {
        //        return BadRequest("User doesn't exist or isn't confirmed");
        //    }

        //    return Ok();
        //}

        //[HttpGet]
        //[Route("google-token/{token}")]
        //public async Task<IActionResult> GoogleToken(string token)
        //{
        //    var profileResponse = await _socialNetworksHelper.GetGoogleDetailsByToken(token);
        //    var email = profileResponse.emails[0].value;
        //    var userName = string.IsNullOrWhiteSpace(profileResponse.displayName.ToString())
        //       ? email
        //       : profileResponse.displayName.ToString();

        //    var userProfileViewModel = new AuthenticationViewModel { Email = email, GoogleProfileId = profileResponse.id.ToString() };

        //    if (profileResponse.name != null)
        //    {
        //        if (profileResponse.name.givenName != null)
        //        {
        //            userProfileViewModel.FirstName = profileResponse.name.givenName;
        //        }

        //        if (profileResponse.name.familyName != null)
        //        {
        //            userProfileViewModel.LastName = profileResponse.name.familyName;
        //        }
        //    }

        //    if (profileResponse.image != null && profileResponse.image.url != null)
        //    {
        //        var imageUrl = profileResponse.image.url.ToString();
        //        userProfileViewModel.PhotoUrl = imageUrl.IndexOf("?") != -1
        //            ? imageUrl.Substring(0, imageUrl.IndexOf("?"))
        //            : imageUrl;
        //    }

        //    var loginResult = await LoginExternal(userProfileViewModel, "google");

        //    if (loginResult.GetType() != typeof(OkObjectResult))
        //    {
        //        return loginResult;
        //    }

        //    return await GetLoginResponse("google", email.ToString());
        //}

        //private async Task<IActionResult> GetLoginResponse(string provider, string email)
        //{
        //    var user = await _userManager.FindByLoginAsync(provider, email);

        //    if (user == null)
        //    {
        //        return BadRequest("User login info not found");
        //    }

        //    var token = _accountService.GenerateJwtToken(email, user);

        //    if (!string.IsNullOrEmpty(token))
        //    {
        //        return Ok(new
        //        {
        //            UserInfo = new
        //            {
        //                user.Id,
        //                user.UserName,
        //                token
        //            },
        //            ReturnUrl = $"{_configuration["AuthCallback"]}?token={token}&returnUrl={_configuration["RedirectUrl"]}"
        //        });
        //    }

        //    return BadRequest("Cannot verify the login. Probably user profile is disabled or deleted.");
        //}
        [Route("LoginExternal")]
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
        [Route("LoginExternal")]
        private IActionResult ProcessExternalLoginResult(Microsoft.AspNetCore.Identity.SignInResult loginResult, string email)
        {
            return Ok(new { loginResult, email });
        }

        //TODO: figure out proper logic for creating user
        [Route("CreateNewUser")]
        private async Task<IdentityResult> CreateNewUser(ApplicationUser newUser, string param, string provider)
        {
            var result = await _userManager.CreateAsync(newUser);
            return result;
        }
    }
}