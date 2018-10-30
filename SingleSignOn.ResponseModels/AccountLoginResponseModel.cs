using SingleSignOn.ViewModels.Account;

namespace SingleSignOn.ResponseModels
{
    public class AccountLoginResponseModel
    {
        public UserInfoViewModel UserInfo { get; set; }
        public string ReturnUrl { get; set; }

        public AccountLoginResponseModel()
        {

        }

        public AccountLoginResponseModel(UserInfoViewModel userInfoViewModel, string returnUrl)
        {
            UserInfo = userInfoViewModel;
            ReturnUrl = returnUrl;
        }
    }
}
