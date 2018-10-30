using System.ComponentModel.DataAnnotations;

namespace SingleSignOn.ViewModels.Account
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
