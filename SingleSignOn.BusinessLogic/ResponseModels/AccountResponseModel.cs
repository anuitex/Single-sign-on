using SingleSignOn.ViewModels.Account;

namespace SingleSignOn.BusinessLogic.ResponseModels.Account
{
    public class AccountResponseModel
    {
        public UserInfoViewModel UserInfo { get; set; }
        public string ReturnUrl { get; set; }
        
        public bool IsOk { get; set; }
        public string Error { get; set; }

        public AccountResponseModel()
        { }

        public AccountResponseModel(UserInfoViewModel userInfoViewModel, string returnUrl)
        {
            UserInfo = userInfoViewModel;
            ReturnUrl = returnUrl;
            IsOk = true;
            Error = "";
        }
    }
}
