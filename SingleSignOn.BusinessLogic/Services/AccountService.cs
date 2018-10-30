using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SingleSignOn.DataAccess.Entities;
using SingleSignOn.BusinessLogic.Interfaces;
using SingleSignOn.DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using SingleSignOn.ViewModels.Account;
using SingleSignOn.Configuration;
using SingleSignOn.Entities;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace SingleSignOn.BusinessLogic.Services
{
    public class AccountService : IAccountService
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        private UserRepository<ApplicationUser> _userRepository;
        public IConfiguration _configuration;

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

        public async Task<IdentityResult> Register(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result;
        }

        public async Task SendEmail(ForgotPasswordViewModel model, string callbackUrl)
        {
            var forgotPasswordEmailConfiguration = new EmailConfiguration();

            EmailProvider _emailProvider = new EmailProvider();

            var emailCredential = new EmailCredential();

            emailCredential.DisplayName = forgotPasswordEmailConfiguration.DisplayName;
            emailCredential.EmailDeliverySmptServer = forgotPasswordEmailConfiguration.EmailDeliverySmptServer;
            emailCredential.EmailDeliveryPort = forgotPasswordEmailConfiguration.EmailDeliveryPort;
            emailCredential.EmailDeliveryLogin = forgotPasswordEmailConfiguration.EmailDeliveryLogin;
            emailCredential.EmailDeliveryPassword = forgotPasswordEmailConfiguration.EmailDeliveryPassword;

            await _emailProvider.SendMessage(emailCredential, forgotPasswordEmailConfiguration.Subject, forgotPasswordEmailConfiguration.ForgotPasswordBodyStart + callbackUrl + forgotPasswordEmailConfiguration.ForgotPasswordBodyEnd, model.Email);
        }
    }
}
