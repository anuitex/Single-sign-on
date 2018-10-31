using System.ComponentModel.DataAnnotations;

namespace SingleSignOn.BusinessLogic.ViewModels.Account
{
    public class EmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
