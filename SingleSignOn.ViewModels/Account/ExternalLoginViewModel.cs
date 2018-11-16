using System.ComponentModel.DataAnnotations;

namespace SingleSignOn.ViewModels.Account
{
    public class ExternalLoginViewModel
    {
        public string Provider { get; set; }
        public string ReturnUrl { get; set; }
    }
}
