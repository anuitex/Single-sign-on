using SingleSignOn.DataAccess.Entities;
using SingleSignOn.ViewModels.Account;

namespace SingleSignOn.ViewModels.Account
{
    public class UserInfoViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }

        public UserInfoViewModel()
        {

        }

        public UserInfoViewModel(ApplicationUser applicationUser, string token = null)
        {
            Id = applicationUser.Id;
            Email = applicationUser.Email;
            Token = token;
        }
    }
}
