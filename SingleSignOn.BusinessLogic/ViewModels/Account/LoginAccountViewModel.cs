using System.ComponentModel.DataAnnotations;

namespace SingleSignOn.BusinessLogic.ViewModels.Account
{
    public class LoginAccountViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
