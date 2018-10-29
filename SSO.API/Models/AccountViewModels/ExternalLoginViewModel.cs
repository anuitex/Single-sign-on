using System.ComponentModel.DataAnnotations;

namespace SSO.API.Models
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
