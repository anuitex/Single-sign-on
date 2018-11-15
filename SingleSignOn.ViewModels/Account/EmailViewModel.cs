using System.ComponentModel.DataAnnotations;

namespace SingleSignOn.ViewModels.Account
{
    public class EmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
