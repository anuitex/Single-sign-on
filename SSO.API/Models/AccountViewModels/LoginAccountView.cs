using System.ComponentModel.DataAnnotations;

namespace SSO.API.Models.AccountViewModels
{
    public class LoginAccountView
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
