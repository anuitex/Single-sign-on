namespace SSO.API.Models.AccountViewModels
{
    public class AccountLoginResponseModel
    {
        public UserInfoViewModel UserInfo { get; set; }
        public string ReturnUrl { get; set; }
    }
}
