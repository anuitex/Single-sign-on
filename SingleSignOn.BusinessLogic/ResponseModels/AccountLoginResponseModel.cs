using SingleSignOn.BusinessLogic.ViewModels.Account;

namespace SingleSignOn.BusinessLogic.ResponseModels.Account
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
