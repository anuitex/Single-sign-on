using System.ComponentModel.DataAnnotations;

namespace SingleSignOn.BusinessLogic.ViewModels.Account
{
    public class RegisterAccountViewModel 
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string ConfirmPassword { get; set; }
        public string ReturnUrl { get; set; }
    }
}
