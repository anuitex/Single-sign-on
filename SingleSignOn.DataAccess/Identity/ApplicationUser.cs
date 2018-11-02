using Microsoft.AspNetCore.Identity;
using System;

namespace SingleSignOn.DataAccess.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhotoUrl { get; set; }
        public string AvatarType { get; set; }
        public string FacebookProfileId { get; set; }
        public string GoogleProfileId { get; set; }
        public string VkProfileId { get; set; }
        public string TwitterProfileId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool AvatarSet { get; set; }
    }
}
