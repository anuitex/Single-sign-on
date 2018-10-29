using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
