using Microsoft.AspNetCore.Identity;

namespace SingleSignOn.BusinessLogic.ViewModels.Account
{
    public class AuthenticationViewModel : IdentityUser
    {
        public string Email { get; set; }
        public string GoogleProfileId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhotoUrl { get; set; }
        public string AvatarType { get; set; }
        public string FacebookProfileId { get; set; }
        public string VkProfileId { get; set; }
        public string TwitterProfileId { get; set; }
    }
}
