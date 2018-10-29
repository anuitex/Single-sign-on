using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SSO.API.Services;
using SSO.DataAccess.Entities;


namespace SSO.API.Controllers
{
    public abstract class BaseAccountController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly IEmailSender _emailSender;
        protected readonly IConfiguration _configuration;

        public BaseAccountController( UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
                                      IEmailSender emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }
    }
}
