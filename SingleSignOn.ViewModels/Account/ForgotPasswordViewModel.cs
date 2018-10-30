using System.ComponentModel.DataAnnotations;

namespace SingleSignOn.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
