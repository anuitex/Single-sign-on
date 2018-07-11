﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.ViewModels
{
    public class AuthenticationViewModel
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
